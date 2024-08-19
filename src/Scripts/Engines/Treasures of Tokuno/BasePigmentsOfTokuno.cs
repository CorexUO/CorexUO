using Server.Misc;
using Server.Mobiles;
using System;

namespace Server.Items
{
	public abstract class BasePigmentsOfTokuno : BaseItem, IUsesRemaining
	{
		private static readonly Type[] m_Glasses = new Type[]
		{
			typeof( MaritimeGlasses ),
			typeof( WizardsGlasses ),
			typeof( TradeGlasses ),
			typeof( LyricalGlasses ),
			typeof( NecromanticGlasses ),
			typeof( LightOfWayGlasses ),
			typeof( FoldedSteelGlasses ),
			typeof( PoisonedGlasses ),
			typeof( TreasureTrinketGlasses ),
			typeof( MaceShieldGlasses ),
			typeof( ArtsGlasses ),
			typeof( AnthropomorphistGlasses )
		};

		private static readonly Type[] m_Replicas = new Type[]
		{
			typeof( ANecromancerShroud ),
			typeof( BraveKnightOfTheBritannia ),
			typeof( CaptainJohnsHat ),
			typeof( DetectiveBoots ),
			typeof( DjinnisRing ),
			typeof( EmbroideredOakLeafCloak ),
			typeof( GuantletsOfAnger ),
			typeof( LieutenantOfTheBritannianRoyalGuard ),
			typeof( OblivionsNeedle ),
			typeof( RoyalGuardSurvivalKnife ),
			typeof( SamaritanRobe ),
			typeof( TheMostKnowledgePerson ),
			typeof( TheRobeOfBritanniaAri ),
			typeof( AcidProofRobe ),
			typeof( Calm ),
			typeof( CrownOfTalKeesh ),
			typeof( FangOfRactus ),
			typeof( GladiatorsCollar ),
			typeof( OrcChieftainHelm ),
			typeof( Pacify ),
			typeof( Quell ),
			typeof( ShroudOfDeciet ),
			typeof( Subdue )
		};

		private static readonly Type[] m_DyableHeritageItems = new Type[]
		{
			typeof( ChargerOfTheFallen ),
			typeof( SamuraiHelm ),
			typeof( HolySword ),
			typeof( LeggingsOfEmbers ),
			typeof( ShaminoCrossbow )
		};

		public override int LabelNumber => 1070933;  // Pigments of Tokuno

		private int m_UsesRemaining;
		private TextDefinition m_Label;

		protected TextDefinition Label
		{
			get => m_Label;
			set { m_Label = value; InvalidateProperties(); }
		}

		public BasePigmentsOfTokuno() : base(0xEFF)
		{
			Weight = 1.0;
			m_UsesRemaining = 1;
		}

		public BasePigmentsOfTokuno(int uses) : base(0xEFF)
		{
			Weight = 1.0;
			m_UsesRemaining = uses;
		}

		public BasePigmentsOfTokuno(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Label != null && m_Label > 0)
				TextDefinition.AddTo(list, m_Label);

			list.Add(1060584, m_UsesRemaining.ToString()); // uses remaining: ~1_val~
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsAccessibleTo(from) && from.InRange(GetWorldLocation(), 3))
			{
				from.SendLocalizedMessage(1070929); // Select the artifact or enhanced magic item to dye.
				from.BeginTarget(3, false, Server.Targeting.TargetFlags.None, new TargetStateCallback(InternalCallback), this);
			}
			else
				from.SendLocalizedMessage(502436); // That is not accessible.
		}

		private void InternalCallback(Mobile from, object targeted, object state)
		{
			BasePigmentsOfTokuno pigment = (BasePigmentsOfTokuno)state;

			if (pigment.Deleted || pigment.UsesRemaining <= 0 || !from.InRange(pigment.GetWorldLocation(), 3) || !pigment.IsAccessibleTo(from))
				return;


			if (targeted is not Item i)
				from.SendLocalizedMessage(1070931); // You can only dye artifacts and enhanced magic items with this tub.
			else if (!from.InRange(i.GetWorldLocation(), 3) || !IsAccessibleTo(from))
				from.SendLocalizedMessage(502436); // That is not accessible.
			else if (from.Items.Contains(i))
				from.SendLocalizedMessage(1070930); // Can't dye artifacts or enhanced magic items that are being worn.
			else if (i.IsLockedDown)
				from.SendLocalizedMessage(1070932); // You may not dye artifacts and enhanced magic items which are locked down.
			else if (i.QuestItem)
				from.SendLocalizedMessage(1151836); // You may not dye toggled quest items.
			else if (i is MetalPigmentsOfTokuno)
				from.SendLocalizedMessage(1042417); // You cannot dye that.
			else if (i is LesserPigmentsOfTokuno)
				from.SendLocalizedMessage(1042417); // You cannot dye that.
			else if (i is PigmentsOfTokuno)
				from.SendLocalizedMessage(1042417); // You cannot dye that.
			else if (!IsValidItem(i))
				from.SendLocalizedMessage(1070931); // You can only dye artifacts and enhanced magic items with this tub.	//Yes, it says tub on OSI.  Don't ask me why ;p
			else
			{
				//Notes: on OSI there IS no hue check to see if it's already hued.  and no messages on successful hue either
				i.Hue = Hue;

				if (--pigment.UsesRemaining <= 0)
					pigment.Delete();

				from.PlaySound(0x23E); // As per OSI TC1
			}
		}

		public static bool IsValidItem(Item i)
		{
			if (i is BasePigmentsOfTokuno)
				return false;

			Type t = i.GetType();

			CraftResource resource = CraftResource.None;

			if (i is BaseWeapon)
				resource = ((BaseWeapon)i).Resource;
			else if (i is BaseArmor)
				resource = ((BaseArmor)i).Resource;
			else if (i is BaseClothing)
				resource = ((BaseClothing)i).Resource;

			if (!CraftResources.IsStandard(resource))
				return true;

			if (i is ITokunoDyable)
				return true;

			return
				IsInTypeList(t, TreasuresOfTokuno.LesserArtifactsTotal)
				|| IsInTypeList(t, TreasuresOfTokuno.GreaterArtifacts)
				|| IsInTypeList(t, DemonKnight.ArtifactRarity10)
				|| IsInTypeList(t, DemonKnight.ArtifactRarity11)
				|| IsInTypeList(t, MondainsLegacy.Artifacts)
				|| IsInTypeList(t, StealableArtifactsSpawner.TypesOfEntires)
				|| IsInTypeList(t, Paragon.Artifacts)
				|| IsInTypeList(t, Leviathan.Artifacts)
				|| IsInTypeList(t, TreasureMapChest.Artifacts)
				|| IsInTypeList(t, m_Replicas)
				|| IsInTypeList(t, m_DyableHeritageItems)
				|| IsInTypeList(t, m_Glasses)
				;
		}

		private static bool IsInTypeList(Type t, Type[] list)
		{
			for (int i = 0; i < list.Length; i++)
			{
				if (list[i] == t) return true;
			}

			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.WriteEncodedInt(m_UsesRemaining);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_UsesRemaining = reader.ReadEncodedInt();
						break;
					}
			}
		}

		#region IUsesRemaining Members

		[CommandProperty(AccessLevel.GameMaster)]
		public int UsesRemaining
		{
			get => m_UsesRemaining;
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		public bool ShowUsesRemaining
		{
			get => true;
			set { }
		}

		#endregion
	}
}
