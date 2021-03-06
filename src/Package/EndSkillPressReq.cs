using NetWork.Utilities;
using ProtoBuf;
using System;

namespace Package
{
	[ForRecv(3800), ForSend(3800), ProtoContract(Name = "EndSkillPressReq")]
	[Serializable]
	public class EndSkillPressReq : IExtensible
	{
		public static readonly short OP = 3800;

		private int _skillId;

		private IExtension extensionObject;

		[ProtoMember(1, IsRequired = true, Name = "skillId", DataFormat = DataFormat.TwosComplement)]
		public int skillId
		{
			get
			{
				return this._skillId;
			}
			set
			{
				this._skillId = value;
			}
		}

		IExtension IExtensible.GetExtensionObject(bool createIfMissing)
		{
			return Extensible.GetExtensionObject(ref this.extensionObject, createIfMissing);
		}
	}
}
