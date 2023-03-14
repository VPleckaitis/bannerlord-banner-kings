﻿using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Utils.Models;
using Bannerlord.UIExtenderEx.Attributes;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Kingdoms
{
    public class InterestGroupVM : BannerKingsViewModel
    {
        private MBBindingList<StringPairItemVM> headers;
        private MBBindingList<StringPairItemVM> secondaryHeaders;
        private MBBindingList<StringPairItemVM> tertiaryHeaders;
        private MBBindingList<GroupMemberVM> members;
        private GroupMemberVM leader;
        private ImageIdentifierVM clanBanner;
        private bool isEmpty, isActionEnabled;
        private string actionName;
        private HintViewModel actionHint;
        private KingdomGroupsVM groupsVM;

        public KingdomDiplomacy KingdomDiplomacy { get; }
        public InterestGroup Group { get; }

        public InterestGroupVM(InterestGroup interestGroup, KingdomGroupsVM groupsVM) : base(null, false)
        {
            Group = interestGroup;
            this.groupsVM = groupsVM;
            KingdomDiplomacy = groupsVM.KingdomDiplomacy;
            Members = new MBBindingList<GroupMemberVM>();
            Headers = new MBBindingList<StringPairItemVM>();
            SecondaryHeaders = new MBBindingList<StringPairItemVM>();
            TertiaryHeaders = new MBBindingList<StringPairItemVM>();
        }

        [DataSourceProperty] public string LeaderText => new TextObject("{=SrfYbg3x}Leader").ToString();
        [DataSourceProperty] public string GroupName => Group.Name.ToString();
        [DataSourceProperty] public HintViewModel Hint => new HintViewModel(Group.Description);

        public void SetGroup()
        {
            groupsVM.SetGroup(this);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Members.Clear();
            Headers.Clear();
            SecondaryHeaders.Clear();
            TertiaryHeaders.Clear();
            IsEmpty = Group.Members.Count == 0;
            if (Group.Leader != null)
            {
                Leader = new GroupMemberVM(Group.Leader, true);
                if (Group.Leader.Clan != null)
                {
                    ClanBanner = new ImageIdentifierVM(BannerCode.CreateFrom(Group.Leader.Clan.Banner), true);
                }
            }

            foreach (var member in Group.GetSortedMembers(KingdomDiplomacy).Take(5))
            {
                if (member != Leader.Hero)
                {
                    Members.Add(new GroupMemberVM(member));
                }
            }

            BKExplainedNumber influence = BannerKingsConfig.Instance.InterestGroupsModel
                .CalculateGroupInfluence(Group, true);

            Headers.Add(new StringPairItemVM(new TextObject("{=EkFaisgP}Influence").ToString(),
                FormatValue(influence.ResultNumber),
                new BasicTooltipViewModel(() => influence.GetFormattedPercentage())));

            BKExplainedNumber support = BannerKingsConfig.Instance.InterestGroupsModel
                .CalculateGroupSupport(Group, true);

            Headers.Add(new StringPairItemVM(new TextObject("{=!}Support").ToString(),
                FormatValue(support.ResultNumber), 
                new BasicTooltipViewModel(() => support.GetFormattedPercentage())));

            Headers.Add(new StringPairItemVM(new TextObject("{=!}Members").ToString(),
                Group.Members.Count.ToString(),
                new BasicTooltipViewModel(() => new TextObject("{=!}The amount of members in this group.").ToString())));

            SecondaryHeaders.Add(new StringPairItemVM(new TextObject("{=!}Endorsed Trait").ToString(),
                Group.MainTrait.Name.ToString(),
                new BasicTooltipViewModel(() => new TextObject("{=!}This group favors those with this personality trait. Hero with this trait are more likely to join the group, and the group supports more a sovereign with this trait.").ToString())));

            SecondaryHeaders.Add(new StringPairItemVM(new TextObject("{=!}Allows Nobility").ToString(),
               GameTexts.FindText(Group.AllowsNobles ? "str_yes" : "str_no").ToString(),
               new BasicTooltipViewModel(() => new TextObject("{=!}Whether or not lords are allowed to participate in this group.").ToString())));

            SecondaryHeaders.Add(new StringPairItemVM(new TextObject("{=!}Allows Commoners").ToString(),
               GameTexts.FindText(Group.AllowsCommoners ? "str_yes" : "str_no").ToString(),
               new BasicTooltipViewModel(() => new TextObject("{=!}Whether or not relevant commoners (notables) are allowed to participate in this group.").ToString())));

            int lords = Group.Members.FindAll(x => x.IsLord).Count;
            int notables = Group.Members.FindAll(x => x.IsNotable).Count;


            TextObject endorsedExplanation = new TextObject("{=!}Laws -----\n{LAWS}\n\n\nPolicies -----\n{POLICIES}\n\n\nCasus Belli -----\n{CASUS}")
                .SetTextVariable("LAWS", Group.SupportedLaws.Aggregate("", (current, law) => 
                current + Environment.NewLine + law.Name))
                .SetTextVariable("POLICIES", Group.SupportedPolicies.Aggregate("", (current, policy) =>
                current + Environment.NewLine + policy.Name))
                .SetTextVariable("CASUS", Group.SupportedCasusBelli.Aggregate("", (current, casus) =>
                current + Environment.NewLine + casus.Name));

            TertiaryHeaders.Add(new StringPairItemVM(new TextObject("{=!}Endorsed Acts").ToString(), 
                string.Empty,
                new BasicTooltipViewModel(() => endorsedExplanation.ToString())));

            TextObject demandsExplanation = new TextObject("{=!}Possible demands\n{DEMANDS}")
                .SetTextVariable("DEMANDS", Group.PossibleDemands.Aggregate("", (current, law) =>
                current + Environment.NewLine + law.Name));

            TertiaryHeaders.Add(new StringPairItemVM(new TextObject("{=!}Demands").ToString(),
                string.Empty,
                new BasicTooltipViewModel(() => demandsExplanation.ToString())));

            TextObject shunnedExplanation = new TextObject("{=!}Laws\n{LAWS}\n\n\nPolicies\n{POLICIES}")
                .SetTextVariable("LAWS", Group.SupportedLaws.Aggregate("", (current, law) =>
                current + Environment.NewLine + law.Name))
                .SetTextVariable("POLICIES", Group.SupportedPolicies.Aggregate("", (current, policy) =>
                current + Environment.NewLine + policy.Name));

            TertiaryHeaders.Add(new StringPairItemVM(new TextObject("{=!}Shunned Acts").ToString(),
                string.Empty,
                new BasicTooltipViewModel(() => shunnedExplanation.ToString())));

            if (Group.Members.Contains(Hero.MainHero))
            {
                ActionName = new TextObject("{=3sRdGQou}Leave").ToString();
                IsActionEnabled = true;
                ActionHint = new HintViewModel(new TextObject("{=!}Leave this group. This will break any ties to their interests and demands. Leaving a group will hurt your relations with it's members, mainly the group leader. If you are the leader yourself, this impact will be increased."));
            }
            else
            {
                ActionName = new TextObject("{=es0Y3Bxc}Join").ToString();
                IsActionEnabled = Group.CanHeroJoin(Hero.MainHero, KingdomDiplomacy);
                ActionHint = new HintViewModel(new TextObject("{=!}Join this group. Being a group member means you will be aligned with their interests and demands. The group leader will be responsible for the group's interaction with the realm's sovereign, and their actions will impact the entire group. For example, a malcontent group leader may make pressure for a member of the group to be awarded a title or property and thus increase the group's influence."));
            }
        }

        [DataSourceMethod]
        private void ExecuteAction()
        {
            if (Group.Members.Contains(Hero.MainHero))
            {
                Group.RemoveMember(Hero.MainHero, KingdomDiplomacy);
            }
            else
            {
                Group.AddMember(Hero.MainHero);
            }
        }

        [DataSourceProperty]
        public bool IsEmpty
        {
            get => isEmpty;
            set
            {
                if (value != isEmpty)
                {
                    isEmpty = value;
                    OnPropertyChangedWithValue(value, "IsEmpty");
                }
            }
        }


        [DataSourceProperty]
        public bool IsActionEnabled
        {
            get => isActionEnabled;
            set
            {
                if (value != isActionEnabled)
                {
                    isActionEnabled = value;
                    OnPropertyChangedWithValue(value, "IsActionEnabled");
                }
            }
        }

        [DataSourceProperty]
        public string ActionName
        {
            get => actionName;
            set
            {
                if (value != actionName)
                {
                    actionName = value;
                    OnPropertyChangedWithValue(value, "ActionName");
                }
            }
        }


        [DataSourceProperty]
        public HintViewModel ActionHint
        {
            get => actionHint;
            set
            {
                if (value != actionHint)
                {
                    actionHint = value;
                    OnPropertyChangedWithValue(value, "ActionHint");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<StringPairItemVM> Headers
        {
            get => headers;
            set
            {
                if (value != headers)
                {
                    headers = value;
                    OnPropertyChangedWithValue(value, "Headers");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<StringPairItemVM> SecondaryHeaders
        {
            get => secondaryHeaders;
            set
            {
                if (value != secondaryHeaders)
                {
                    secondaryHeaders = value;
                    OnPropertyChangedWithValue(value, "SecondaryHeaders");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<StringPairItemVM> TertiaryHeaders
        {
            get => tertiaryHeaders;
            set
            {
                if (value != tertiaryHeaders)
                {
                    tertiaryHeaders = value;
                    OnPropertyChangedWithValue(value, "TertiaryHeaders");
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM ClanBanner
        {
            get => clanBanner;
            set
            {
                if (value != clanBanner)
                {
                    clanBanner = value;
                    OnPropertyChangedWithValue(value, "ClanBanner");
                }
            }
        }

        [DataSourceProperty]
        public GroupMemberVM Leader
        {
            get => leader;
            set
            {
                if (value != leader)
                {
                    leader = value;
                    OnPropertyChangedWithValue(value, "Leader");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<GroupMemberVM> Members
        {
            get => members;
            set
            {
                if (value != members)
                {
                    members = value;
                    OnPropertyChangedWithValue(value, "Members");
                }
            }
        }
    }
}
