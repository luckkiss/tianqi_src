using ProtoBuf;
using System;
using System.ComponentModel;

namespace GameData
{
	[ProtoContract(Name = "ShengXingCaiLiaoHeCheng")]
	[Serializable]
	public class ShengXingCaiLiaoHeCheng : IExtensible
	{
		private int _starId;

		private int _needId;

		private int _composeAmount;

		private int _afterId;

		private IExtension extensionObject;

		[ProtoMember(2, IsRequired = true, Name = "starId", DataFormat = DataFormat.TwosComplement)]
		public int starId
		{
			get
			{
				return this._starId;
			}
			set
			{
				this._starId = value;
			}
		}

		[ProtoMember(3, IsRequired = false, Name = "needId", DataFormat = DataFormat.TwosComplement), DefaultValue(0)]
		public int needId
		{
			get
			{
				return this._needId;
			}
			set
			{
				this._needId = value;
			}
		}

		[ProtoMember(4, IsRequired = false, Name = "composeAmount", DataFormat = DataFormat.TwosComplement), DefaultValue(0)]
		public int composeAmount
		{
			get
			{
				return this._composeAmount;
			}
			set
			{
				this._composeAmount = value;
			}
		}

		[ProtoMember(5, IsRequired = false, Name = "afterId", DataFormat = DataFormat.TwosComplement), DefaultValue(0)]
		public int afterId
		{
			get
			{
				return this._afterId;
			}
			set
			{
				this._afterId = value;
			}
		}

		IExtension IExtensible.GetExtensionObject(bool createIfMissing)
		{
			return Extensible.GetExtensionObject(ref this.extensionObject, createIfMissing);
		}
	}
}
