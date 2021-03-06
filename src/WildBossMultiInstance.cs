using GameData;
using Package;
using System;
using System.Collections.Generic;
using UnityEngine;
using XNetwork;

public class WildBossMultiInstance : BattleInstanceParent<ResultWildBossNty>
{
	protected static WildBossMultiInstance instance;

	protected float currentCollectDropTime;

	protected TimeCountDown timeCountDown;

	protected uint endCameraTimer;

	protected uint showWinUITimer;

	protected uint showWinPoseTimer;

	protected uint autoExitTimer;

	public static WildBossMultiInstance Instance
	{
		get
		{
			if (WildBossMultiInstance.instance == null)
			{
				WildBossMultiInstance.instance = new WildBossMultiInstance();
			}
			return WildBossMultiInstance.instance;
		}
	}

	public override float CurrentCollectDropTime
	{
		get
		{
			return this.currentCollectDropTime;
		}
	}

	protected WildBossMultiInstance()
	{
		base.Type = InstanceType.WildBossMulti;
		this.currentCollectDropTime = base.CurrentCollectDropTime;
	}

	public override void AddInstanceListeners()
	{
		base.AddInstanceListeners();
		NetworkManager.AddListenEvent<WildBossPreSettleNty>(new NetCallBackMethod<WildBossPreSettleNty>(this.GetInstancePreResult));
	}

	public override void RemoveInstanceListeners()
	{
		base.RemoveInstanceListeners();
		NetworkManager.RemoveListenEvent<WildBossPreSettleNty>(new NetCallBackMethod<WildBossPreSettleNty>(this.GetInstancePreResult));
	}

	public override void ReleaseData()
	{
		base.ReleaseData();
		this.currentCollectDropTime = base.CurrentCollectDropTime;
	}

	public override bool IsShowMonsterBorn(int monsterID)
	{
		return DataReader<Monster>.Get(monsterID).birthAction == 0;
	}

	public override void ShowBattleUI()
	{
		if (base.InstanceResult != null)
		{
			return;
		}
		BattleUI battleUI = LinkNavigationManager.OpenBattleUI();
		battleUI.BtnQuitAction = delegate
		{
			UIManagerControl.Instance.OpenUI("GlobalBattleDialogUI", UINodesManager.MiddleUIRoot, false, UIType.NonPush);
			GlobalBattleDialogUIViewModel.Instance.ShowAsOKCancel(GameDataUtils.GetChineseContent(510109, false), GameDataUtils.GetChineseContent(505191, false), delegate
			{
			}, delegate
			{
				WildBossManager.Instance.InitiativeQuit();
			}, GameDataUtils.GetChineseContent(621272, false), GameDataUtils.GetChineseContent(621271, false), "button_orange_1", "button_yellow_1", null, true);
			GlobalBattleDialogUIView.Instance.isClick = false;
		};
		battleUI.ResetAllInstancePart();
		battleUI.ShowDamageRankingUI(true);
		battleUI.IsPauseCheck = false;
		battleUI.IsInAuto = (base.InstanceData.autoFight == 0);
	}

	public override void GiveUpRelive()
	{
		WildBossManager.Instance.ExitWildBoss();
	}

	protected void GetInstancePreResult(short state, WildBossPreSettleNty down = null)
	{
		Debug.LogError(string.Concat(new object[]
		{
			"down: ",
			down.pickTime,
			" ",
			down.countdown
		}));
		if (state != 0)
		{
			StateManager.Instance.StateShow(state, 0);
			return;
		}
		if (down == null)
		{
			return;
		}
		this.currentCollectDropTime = (float)down.pickTime;
		InstanceManager.SetCollectCountDown(down.countdown);
	}

	public override void GetInstanceResult(ResultWildBossNty result)
	{
		if (InstanceManager.CurrentInstanceType != base.Type)
		{
			return;
		}
		this.instanceResult = result;
		InstanceManager.SetInstanceDrop(this.FixDropData(base.InstanceResult.item));
		if (result.isWin)
		{
			InstanceManager.InstanceWin();
		}
		else
		{
			if (!string.IsNullOrEmpty(result.name))
			{
				UIManagerControl.Instance.ShowToastText(string.Format(GameDataUtils.GetChineseContent(505175, false), result.name), 1f, 1f);
			}
			InstanceManager.InstanceLose();
		}
	}

	protected List<KeyValuePair<int, long>> FixDropData(List<DropItem> items)
	{
		List<KeyValuePair<int, long>> list = new List<KeyValuePair<int, long>>();
		if (items != null && items.get_Count() > 0)
		{
			for (int i = 0; i < items.get_Count(); i++)
			{
				list.Add(new KeyValuePair<int, long>(items.get_Item(i).typeId, items.get_Item(i).count));
			}
		}
		return list;
	}

	public override void EndingCountdown(Action onCountdownEnd)
	{
		if (!InstanceManager.IsShowInstanceDrop && !InstanceManager.IsShowMonsterDrop)
		{
			if (onCountdownEnd != null)
			{
				onCountdownEnd.Invoke();
			}
			return;
		}
		if (base.InstanceData.endTime <= 1000)
		{
			if (onCountdownEnd != null)
			{
				onCountdownEnd.Invoke();
			}
			return;
		}
		float cameraTime = float.Parse(DataReader<GlobalParams>.Get("instanceEndCameraTime").value);
		this.endCameraTimer = TimerHeap.AddTimer((uint)cameraTime, 0, delegate
		{
			float num = (float)this.InstanceData.endTime - cameraTime;
			if (num <= 1000f)
			{
				if (onCountdownEnd != null)
				{
					onCountdownEnd.Invoke();
				}
				return;
			}
			UIManagerControl.Instance.OpenUI("DungeonCountDownUI", UINodesManager.MiddleUIRoot, false, UIType.NonPush);
			if (DungeonCountDownUI.Instance != null)
			{
				DungeonCountDownUI.Instance.UpdateNum((int)(num * 0.001f));
			}
			this.timeCountDown = new TimeCountDown((int)(num * 0.001f), TimeFormat.SECOND, delegate
			{
				if (DungeonCountDownUI.Instance != null)
				{
					DungeonCountDownUI.Instance.UpdateNum(this.timeCountDown.GetSeconds());
				}
			}, delegate
			{
				this.timeCountDown.Dispose();
				this.timeCountDown = null;
				UIManagerControl.Instance.UnLoadUIPrefab("DungeonCountDownUI");
				if (onCountdownEnd != null)
				{
					onCountdownEnd.Invoke();
				}
			}, true);
		});
	}

	public override void ShowWinPose()
	{
		this.showWinPoseTimer = TimerHeap.AddTimer(2000u, 0, delegate
		{
			base.ShowWinPose();
		});
	}

	public override void ShowWinUI()
	{
		base.ShowWinUI();
		UIManagerControl.Instance.HideUI("BattleUI");
		this.showWinUITimer = TimerHeap.AddTimer(5500u, 0, delegate
		{
			CommonBattlePassUI commonBattlePassUI = LinkNavigationManager.OpenCommonBattlePassUI();
			commonBattlePassUI.PlayAnimation(InstanceResultType.Win);
			commonBattlePassUI.BtnStatictisVisibity = false;
			commonBattlePassUI.BtnAgainVisibility = false;
			commonBattlePassUI.BtnMultipleVisibility = false;
			commonBattlePassUI.BtnTipTextVisibility = true;
			commonBattlePassUI.SetDropItems(this.FixInstanceDrop());
			commonBattlePassUI.ExitCallback = delegate
			{
				WildBossManager.Instance.ExitWildBoss();
			};
			commonBattlePassUI.OnCountDownToExit(5, delegate
			{
				WildBossManager.Instance.ExitWildBoss();
			});
		});
	}

	protected PassUICommonDrop FixInstanceDrop()
	{
		PassUICommonDrop passUICommonDrop = new PassUICommonDrop();
		XDict<int, long> xDict = new XDict<int, long>();
		for (int i = 0; i < base.InstanceResult.item.get_Count(); i++)
		{
			int typeId = base.InstanceResult.item.get_Item(i).typeId;
			if (typeId != 1)
			{
				if (typeId != 2)
				{
					if (xDict.ContainsKey(base.InstanceResult.item.get_Item(i).typeId))
					{
						XDict<int, long> xDict2;
						XDict<int, long> expr_B2 = xDict2 = xDict;
						int typeId2;
						int expr_CB = typeId2 = base.InstanceResult.item.get_Item(i).typeId;
						long num = xDict2[typeId2];
						expr_B2[expr_CB] = num + base.InstanceResult.item.get_Item(i).count;
					}
					else
					{
						xDict.Add(base.InstanceResult.item.get_Item(i).typeId, base.InstanceResult.item.get_Item(i).count);
					}
				}
				else
				{
					passUICommonDrop.gold += base.InstanceResult.item.get_Item(i).count;
				}
			}
			else
			{
				passUICommonDrop.exp += base.InstanceResult.item.get_Item(i).count;
			}
		}
		for (int j = 0; j < xDict.Count; j++)
		{
			passUICommonDrop.item.Add(new KeyValuePair<int, long>(xDict.ElementKeyAt(j), xDict.ElementValueAt(j)));
		}
		return passUICommonDrop;
	}

	public override void ShowLoseUI()
	{
		base.ShowLoseUI();
		this.autoExitTimer = TimerHeap.AddTimer(5000u, 0, delegate
		{
			WildBossManager.Instance.ExitWildBoss();
		});
		BattleLoseUI battleLoseUI = LinkNavigationManager.OpenBattleLoseUI();
		battleLoseUI.ShowBtnAgainBtn(false);
		battleLoseUI.ShowBtnDamageCal(false);
		battleLoseUI.ShowRecommendPower(false, 0);
		battleLoseUI.BtnExitAction = delegate
		{
			TimerHeap.DelTimer(this.autoExitTimer);
			WildBossManager.Instance.ExitWildBoss();
		};
		battleLoseUI.BtnAgainAction = null;
		battleLoseUI.BtnEquipQuaAction = delegate
		{
			SceneLoadedUISetting.CurrentType = "SHOW_EQUIPQUALITY";
			WildBossManager.Instance.ExitWildBoss();
		};
		battleLoseUI.BtnEquipLvAction = delegate
		{
			SceneLoadedUISetting.CurrentType = "SHOW_EQUIPLEVEL";
			WildBossManager.Instance.ExitWildBoss();
		};
		battleLoseUI.BtnGemLvAction = delegate
		{
			SceneLoadedUISetting.CurrentType = "SHOW_GEMLEVEL";
			WildBossManager.Instance.ExitWildBoss();
		};
		battleLoseUI.BtnSkillAction = delegate
		{
			SceneLoadedUISetting.CurrentType = "SHOW_SKILL";
			WildBossManager.Instance.ExitWildBoss();
		};
		battleLoseUI.BtnPetLvAction = delegate
		{
			SceneLoadedUISetting.CurrentType = "SHOW_PETLEVEL";
			WildBossManager.Instance.ExitWildBoss();
		};
		battleLoseUI.BtnPetStarAction = delegate
		{
			SceneLoadedUISetting.CurrentType = "SHOW_PETSTRA";
			WildBossManager.Instance.ExitWildBoss();
		};
		battleLoseUI.BtnPetSkillAction = delegate
		{
			SceneLoadedUISetting.CurrentType = "SHOW_PETSKILL";
			WildBossManager.Instance.ExitWildBoss();
		};
		battleLoseUI.BtnGodSoldierAction = delegate
		{
			SceneLoadedUISetting.CurrentType = "SHOW_GODSOLDIER";
			WildBossManager.Instance.ExitWildBoss();
		};
		battleLoseUI.BtnWingAction = delegate
		{
			SceneLoadedUISetting.CurrentType = "SHOW_WING";
			WildBossManager.Instance.ExitWildBoss();
		};
	}

	public override void ExitBattleField()
	{
		TimerHeap.DelTimer(this.endCameraTimer);
		TimerHeap.DelTimer(this.showWinPoseTimer);
		TimerHeap.DelTimer(this.showWinUITimer);
		TimerHeap.DelTimer(this.autoExitTimer);
		base.ExitBattleField();
	}
}
