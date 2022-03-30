﻿using BannerKings.Managers.Kingdoms;
using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Managers.Policies;
using BannerKings.Models;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.UI
{
    public class DemesneVM : BannerKingsViewModel
    {
		private HeroVM deJure;
		private MBBindingList<InformationElement> demesneInfo, landInfo, terrainInfo, workforceInfo, governmentInfo;
		private FeudalTitle title;
		private FeudalTitle duchy;
		private bool _contractEnabled;
		private SelectorVM<BKItemVM> workforceVM;
		private BKWorkforcePolicy workforceItem;

		public DemesneVM(PopulationData data, FeudalTitle title, bool isSelected) : base(data, isSelected)
        {
			this.title = title;
			if (title != null)
            {
				this.deJure = new HeroVM(title.deJure, false);
				this.duchy = BannerKingsConfig.Instance.TitleManager.GetDuchy(this.title);
				this._contractEnabled = BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(Hero.MainHero) && Clan.PlayerClan.Kingdom != null;
			}

			this.demesneInfo = new MBBindingList<InformationElement>();
			this.governmentInfo = new MBBindingList<InformationElement>();
			this.landInfo = new MBBindingList<InformationElement>();
			this.terrainInfo = new MBBindingList<InformationElement>();
			this.workforceInfo = new MBBindingList<InformationElement>();
			
			/*
			
			this.duchyCosts = model.GetUsurpationCosts(_duchy, Hero.MainHero);
			
			this._usurpDuchyEnabled = this._duchy.deJure != Hero.MainHero;
			if (title.vassals != null)
				foreach (FeudalTitle vassal in title.vassals)
					if (vassal.fief != null) _vassals.Add(new VassalTitleVM(vassal)); */
		}

		public override void RefreshValues()
        {
			base.RefreshValues();
			LandData landData = base.data.LandData;
			DemesneInfo.Clear();
			LandInfo.Clear();
			TerrainInfo.Clear();
			WorkforceInfo.Clear();
			GovernmentInfo.Clear();

			if (title != null)
            {
				float legitimacyType = new BKLegitimacyModel().CalculateEffect(base.data.Settlement).ResultNumber;
				if (legitimacyType != 0f)
				{
					LegitimacyType legitimacy = (LegitimacyType)legitimacyType;
					DemesneInfo.Add(new InformationElement("Legitimacy:", legitimacy.ToString().Replace('_', ' '),
						"Your legitimacy to this title and it's vassals. You are lawful when you own this title, and considered a foreigner if your culture differs from it."));
				}

				if (title.sovereign != null) DemesneInfo.Add(new InformationElement("Sovereign:", title.sovereign.name.ToString(),
					"The master suzerain of this title, be they a king or emperor type suzerain."));
				if (duchy != null) DemesneInfo.Add(new InformationElement("Dukedom:", duchy.name.ToString(),
					"The dukedom this settlement is associated with."));

				GovernmentInfo.Add(new InformationElement("Government Type:", title.contract.Government.ToString(),
					"The dukedom this settlement is associated with."));
				GovernmentInfo.Add(new InformationElement("Succession Type:", title.contract.Succession.ToString().Replace("_", " "),
					"The dukedom this settlement is associated with."));
				GovernmentInfo.Add(new InformationElement("Inheritance Type:", title.contract.Inheritance.ToString(),
					"The dukedom this settlement is associated with."));
				GovernmentInfo.Add(new InformationElement("Gender Law:", title.contract.GenderLaw.ToString(),
					"The dukedom this settlement is associated with."));

				DeJure = new HeroVM(title.deJure, false);
			}
			

			LandInfo.Add(new InformationElement("Acreage:", landData.Acreage.ToString() + " acres",
				"Current quantity of usable acres in this region"));
			LandInfo.Add(new InformationElement("Farmland:", landData.Farmland.ToString() + " acres",
				"Acres in this region used as farmland, the main source of food in most places"));
			LandInfo.Add(new InformationElement("Pastureland:", landData.Pastureland.ToString() + " acres",
				"Acres in this region used as pastureland, to raise cattle and other animals. These output meat and animal products such as butter and cheese"));
			LandInfo.Add(new InformationElement("Woodland:", landData.Woodland.ToString() + " acres",
				"Acres in this region used as woodland, kept for hunting, foraging of berries and materials like wood"));

			TerrainInfo.Add(new InformationElement("Type:", landData.Terrain.ToString(),
				"The local terrain type. Dictates fertility and terrain difficulty."));
			TerrainInfo.Add(new InformationElement("Fertility:", base.FormatValue(landData.Fertility),
				"How fertile the region is. This depends solely on the local terrain type - harsher environments like deserts are less fertile than plains and grassy hills"));
			TerrainInfo.Add(new InformationElement("Terrain Difficulty:", base.FormatValue(landData.Difficulty),
				"Represents how difficult it is to create new usable acres. Like fertility, depends on terrain, but is not strictly correlated to it"));

			WorkforceInfo.Add(new InformationElement("Available Workforce:", landData.AvailableWorkForce.ToString(),
				"The amount of productive workers in this region, able to work the land"));
			WorkforceInfo.Add(new InformationElement("Workforce Saturation:", base.FormatValue(landData.WorkforceSaturation),
				"Represents how many workers there are in correlation to the amount needed to fully utilize the acreage. Saturation over 100% indicates more workers than the land needs, while under 100% means not all acres are producing output"));

			if (base.HasTown)
			{
				workforceItem = (BKWorkforcePolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(data.Settlement, "workforce");
				WorkforceSelector = base.GetSelector(workforceItem, new Action<SelectorVM<BKItemVM>>(this.workforceItem.OnChange));
				WorkforceSelector.SelectedIndex = workforceItem.Selected;
				WorkforceSelector.SetOnChangeAction(this.workforceItem.OnChange);
			}
		}

		[DataSourceProperty]
		public SelectorVM<BKItemVM> WorkforceSelector
		{
			get => this.workforceVM;
			set
			{
				if (value != this.workforceVM)
				{
					this.workforceVM = value;
					base.OnPropertyChangedWithValue(value, "WorkforceSelector");
				}
			}
		}

		
		private void OnUsurpPress()
		{
			if (title != null)
            {
				Kingdom kingdom = this.data.Settlement.OwnerClan.Kingdom;
				if (kingdom != null)
					kingdom.AddDecision(new BKGovernmentDecision(this.data.Settlement.OwnerClan, GovernmentType.Imperial, this.title));
			}
		}

		/*
		private void OnDuchyUsurpPress()
		{
			bool usurpable = _duchyUsurpable.Item1;
			if (usurpable)
				BannerKingsConfig.Instance.TitleManager.UsurpTitle(_duchy.deJure, Hero.MainHero, _duchy, duchyCosts);
			else InformationManager.DisplayMessage(new InformationMessage(_duchyUsurpable.Item2));
			RefreshValues();
		}

		private void OnSuzerainPress()
        {
			FeudalTitle suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(Hero.MainHero);
			if (suzerain != null)
				Campaign.Current.EncyclopediaManager.GoToLink(suzerain.deJure.EncyclopediaLink);
			else InformationManager.DisplayMessage(new InformationMessage("You currently have no suzerain."));
		}

		private void OnVassalsPress()
        {
			List<InquiryElement> list = new List<InquiryElement>();
			HashSet<FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetTitles(Hero.MainHero);
			foreach (FeudalTitle title in titles)
            {
				if (title.vassals != null )
					foreach (FeudalTitle vassal in title.vassals)
					{
						Hero deJure = vassal.deJure;
						if (deJure != Hero.MainHero && (deJure.Clan.Kingdom == Clan.PlayerClan.Kingdom || deJure.Clan == Clan.PlayerClan))
						{
							FeudalTitle deJureSuzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(deJure);
							if (deJureSuzerain != null && deJureSuzerain.deFacto == Hero.MainHero)
								list.Add(new InquiryElement(deJure, deJure.Name.ToString(),
									new ImageIdentifier(CampaignUIHelper.GetCharacterCode(deJure.CharacterObject, false))
								));
						}
					}	
			}

			if (list.Count > 0)
				InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
					"Your direct vassals", "You are the legal suzerain of these lords. They must fulfill their duties towards you and you uphold their rights.", list, true, 1,
					GameTexts.FindText("str_done", null).ToString(), "", null, null, ""), false);
			else InformationManager.DisplayMessage(new InformationMessage("You currently have no vassals."));
		}

		private void OnContractPress()
        {
			Kingdom kingdom = _title.fief.OwnerClan.Kingdom;
			if (kingdom != null)
				BannerKingsConfig.Instance.TitleManager.ShowContract(kingdom.Leader, "Close");
			else InformationManager.DisplayMessage(new InformationMessage("Unable to open contract: no kingdom associated with this title."));
		} */


		[DataSourceProperty]
		public MBBindingList<InformationElement> LandInfo
		{
			get => landInfo;
			set
			{
				if (value != landInfo)
				{
					landInfo = value;
					base.OnPropertyChangedWithValue(value, "LandInfo");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> TerrainInfo
		{
			get => terrainInfo;
			set
			{
				if (value != terrainInfo)
				{
					terrainInfo = value;
					base.OnPropertyChangedWithValue(value, "TerrainInfo");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> WorkforceInfo
		{
			get => workforceInfo;
			set
			{
				if (value != workforceInfo)
				{
					workforceInfo = value;
					base.OnPropertyChangedWithValue(value, "WorkforceInfo");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> GovernmentInfo
		{
			get => governmentInfo;
			set
			{
				if (value != governmentInfo)
				{
					governmentInfo = value;
					base.OnPropertyChangedWithValue(value, "GovernmentInfo");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> DemesneInfo
		{
			get => demesneInfo;
			set
			{
				if (value != demesneInfo)
				{
					demesneInfo = value;
					base.OnPropertyChangedWithValue(value, "DemesneInfo");
				}
			}
		}


		[DataSourceProperty]
		public HeroVM DeJure
		{
			get => this.deJure;
			set
			{
				if (value != this.deJure)
				{
					this.deJure = value;
					base.OnPropertyChangedWithValue(value, "DeJure");
				}
			}
		}

		/*

		

		[DataSourceProperty]
		public HintViewModel UsurpDuchyHint
		{
			get
			{
				StringBuilder sb = new StringBuilder("Usurp the duchy associated with this settlement, making you the lawful suzerain of any other lords within the duchy that are not dukes or higher themselves. Usurping a duchy requires the ownership of at least one of the counties within it.");
				sb.Append(Environment.NewLine);
				sb.Append("Costs:");
				sb.Append(Environment.NewLine);
				sb.Append(string.Format("{0} gold.", (int)duchyCosts.gold));
				sb.Append(Environment.NewLine);
				sb.Append(string.Format("{0} influence.", (int)duchyCosts.influence));
				sb.Append(Environment.NewLine);
				sb.Append(string.Format("{0} renown.", (int)duchyCosts.renown));
				return new HintViewModel(new TextObject(sb.ToString()));
			}
		}*/
	}
}