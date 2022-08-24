using System;
using System.IO;
using System.Linq;
using System.Xml;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;
using static TaleWorlds.Core.ItemCategory;

namespace BannerKings.Utils
{
    public static class Helpers
    {
        public static BuildingType _buildingCastleRetinue = Game.Current.ObjectManager.RegisterPresumedObject(new BuildingType("building_castle_retinue"));

        public static void AddSellerToKeep(Hero seller, Settlement settlement)
        {
            var agent = new AgentData(new SimpleAgentOrigin(seller.CharacterObject, 0));
            var locCharacter = new LocationCharacter(agent, SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, null, true, LocationCharacter.CharacterRelations.Neutral, null, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; }, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
        }


        public static bool IsClanLeader(Hero hero)
        {
            return hero.Clan != null && hero.Clan.Leader == hero;
        }

        public static bool IsCloseFamily(Hero hero, Hero family)
        {
            return hero.Father == family || hero.Mother == family || hero.Children.Contains(family) ||
                   hero.Siblings.Contains(family);
        }

        public static int GetRosterCount(TroopRoster roster, string filter = null)
        {
            var rosters = roster.GetTroopRoster();
            var count = 0;

            rosters.ForEach(rosterElement =>
            {
                if (filter == null)
                {
                    if (!rosterElement.Character.IsHero)
                    {
                        count += rosterElement.Number + rosterElement.WoundedNumber;
                    }
                }
                else if (!rosterElement.Character.IsHero && rosterElement.Character.StringId.Contains(filter))
                {
                    count += rosterElement.Number + rosterElement.WoundedNumber;
                }
            });

            return count;
        }

        public static TextObject GetClassName(PopType type, CultureObject culture)
        {
            var cultureModifier = '_' + culture.StringId;
            var id = $"pop_class_{type.ToString().ToLower()}{cultureModifier}";
            var text = type.ToString();
            switch (type)
            {
                case PopType.Serfs when culture.StringId == "sturgia":
                    text = "Lowmen";
                    break;
                case PopType.Serfs when culture.StringId is "empire" or "aserai":
                    text = "Commoners";
                    break;
                case PopType.Serfs when culture.StringId == "battania":
                    text = "Freemen";
                    break;
                case PopType.Serfs:
                {
                    if (culture.StringId == "khuzait")
                    {
                        text = "Nomads";
                    }

                    break;
                }
                case PopType.Slaves when culture.StringId == "sturgia":
                    text = "Thralls";
                    break;
                case PopType.Slaves:
                {
                    if (culture.StringId == "aserai")
                    {
                        text = "Mameluke";
                    }

                    break;
                }
                case PopType.Craftsmen:
                {
                    if (culture.StringId is "khuzait" or "battania")
                    {
                        text = "Artisans";
                    }

                    break;
                }
                case PopType.Nobles when culture.StringId == "empire":
                    text = "Nobiles";
                    break;
                case PopType.Nobles when culture.StringId == "sturgia":
                    text = "Knyaz";
                    break;
                case PopType.Nobles:
                {
                    if (culture.StringId == "vlandia")
                    {
                        text = "Ealdormen";
                    }

                    break;
                }
                case PopType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            var finalResult = $"{{={id}}}{text}";
            return new TextObject(finalResult);
        }

        public static string GetGovernmentDescription(GovernmentType type)
        {
            var text = type switch
            {
                GovernmentType.Imperial => new TextObject("{=x9KpSiZd}An Imperial government is a highly centralized one. Policies favor the ruling clan at the expense of vassals. A strong leadership that sees it's vassals more as administrators than lords."),
                GovernmentType.Tribal => new TextObject("{=MJqMOt9A}The Tribal association is the most descentralized government. Policies to favor the ruling clan are unwelcome, and every lord is a 'king' or 'queen' in their own right."),
                GovernmentType.Republic => new TextObject("{=pxtR9daj}Republics are firmly setup to avoid the accumulation of power. Every clan is given a chance to rule, and though are able to have a few political advantages, the state is always the priority."),
                _ => new TextObject("{=cECbfV8E}Feudal societies can be seen as the midway between tribals and imperials. Although the ruling clan accumulates privileges, and often cannot be easily removed from the throne, lords and their rightful property need to be respected.")
            };

            return text.ToString();
        }

        public static string GetSuccessionTypeDescription(SuccessionType type)
        {
            var text = type switch
            {
                SuccessionType.Elective_Monarchy => new TextObject("{=ZBO27khX}In elective monarchies, the ruler is chosen from the realm's dynasties, and rules until death or abdication. Elections take place and all dynasties are able to vote when a new leader is required."),
                SuccessionType.Hereditary_Monarchy => new TextObject("{=9JOsApQy}In hereditary monarchies, the monarch is always the ruling dynasty's leader. No election takes place, and the realm does not change leadership without extraordinary measures."),
                SuccessionType.Imperial => new TextObject("{=Up28ojKU}Imperial successions are completely dictated by the emperor/empress. They will choose from most competent members in their family, as well as other family leaders. Imperial succession values age, family prestigy, military and administration skills. No election takes place."),
                _ => new TextObject("{=jkNNf870}Republican successions ensure the power is never concentrated. Each year, a new ruler is chosen from the realm's dynasties. The previous ruler is strickly forbidden to participate. Age, family prestige and administration skills are sought after in candidates.")
            };

            return text.ToString();
        }

        public static string GetSuccessionTypeName(SuccessionType type)
        {
            var text = type switch
            {
                SuccessionType.Elective_Monarchy => new TextObject("{=KmwNfHDE}Elective Monarchy"),
                SuccessionType.Hereditary_Monarchy => new TextObject("{=QWR1Psjx}Hereditary Monarchy"),
                SuccessionType.Imperial => new TextObject("{=yOH1d9a0}Imperial"),
                _ => new TextObject("{=23SWYU1A}Republican")
            };

            return text.ToString();
        }

        public static string GetInheritanceDescription(InheritanceType type)
        {
            var text = type switch
            {
                InheritanceType.Primogeniture => new TextObject("{=LYhY3DSM}Primogeniture favors blood family of eldest age. Clan members not related by blood are last resort."),
                InheritanceType.Seniority => new TextObject("{=kxtSSSF2}Seniority favors those of more advanced age in the clan, regardless of blood connections."),
                _ => new TextObject("{=BZZS1GXK}Ultimogeniture favors the youngest in the clan, as well as blood family. Clan members not related by blood are last resort.")
            };

            return text.ToString();
        }

        public static string GetGenderLawDescription(GenderLaw type)
        {
            return type == GenderLaw.Agnatic 
                ? new TextObject("{=L6Vn0rr5}Agnatic law favors males. Although females are not completely excluded, they will only be chosen in case a male candidate is not present.").ToString() 
                : new TextObject("{=fQr0KL4u}Cognatic law sees no distinction between both genders. Candidates are choosen stricly on their merits, as per the context requires.").ToString();
        }

        public static string GetClassHint(PopType type, CultureObject culture)
        {
            var name = GetClassName(type, culture).ToString();
            var description = type switch
            {
                PopType.Nobles => " represent the free, wealthy and influential members of society. They pay very high taxes and increase your influence as a lord.",
                PopType.Craftsmen => " are free people of trade, such as merchants, engineers and blacksmiths. Somewhat wealthy, free but not high status people. Craftsmen pay a significant amount of taxes and their presence boosts economical development. Their skills can also be hired to significantly boost construction projects.",
                PopType.Serfs => " are the lowest class that possess some sort of freedom. Unable to attain specialized skills such as those of craftsmen, these people represent the agricultural workforce. They also pay tax over the profit of their production excess.",
                _ => " are those destituted: criminals, prisioners unworthy of a ransom, and those unlucky to be born into slavery. Slaves do the hard manual labor across settlements, such as building and mining. They themselves pay no tax as they are unable to have posessions, but their labor generates income gathered as tax from their masters."
            };

            return name + description;
        }

        public static string GetConsumptionHint(ConsumptionType type)
        {
            return type switch
            {
                ConsumptionType.Luxury => "Satisfaction over availability of products such as jewelry, velvets and fur.",
                ConsumptionType.Industrial => "Satisfaction over availability of manufacturing products such as leather, clay and tools.",
                ConsumptionType.General => "Satisfaction over availability of various products, including military equipment and horses.",
                _ => "Satisfaction over availability of food types."
            };
        }

        public static string GetTitleHonorary(TitleType type, GovernmentType government, bool female, CultureObject culture = null)
        {
            TextObject title = null;
            if (culture != null)
            {
                switch (culture.StringId)
                {
                    case "battania" when type == TitleType.Kingdom:
                    {
                        title = female 
                            ? new TextObject("{=55QqRJhj}Ard-Banrigh") 
                            : new TextObject("{=xaV94jKE}{MALE}Ard-Rìgh{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=BXO6tqV3}Banrigh")
                            : new TextObject("{=GxwCmPs0}{MALE}Rìgh{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=epW50s3B}Bantiarna") 
                            : new TextObject("{=ZvzZWsB5}{MALE}Mormaer{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=Gmzx67BP}Thaoiseach") 
                            : new TextObject("{=jWidWe7h}{MALE}Toisiche{?}Queen{\\?}");

                        break;
                    }
                    case "battania" when female:
                        title = new TextObject("{=teePOOOU}Baintighearna");
                        break;
                    case "battania":
                        title = new TextObject("{=eMh815hZ}{MALE}Tighearna{?}Queen{\\?}");
                        break;
                    case "empire" when type == TitleType.Kingdom:
                    {
                        if (government == GovernmentType.Republic)
                        {
                            title = female
                                ? new TextObject("{=w16Aa44g}Principissa")
                                : new TextObject("{=oGVdJap4}Princeps");
                        }
                        else
                        {
                            title = female 
                                ? new TextObject("{=xRr1Nws2}Regina") 
                                : new TextObject("{=XfTvaDYp}{MALE}Rex{?}Queen{\\?}");
                        }

                        break;
                    }
                    case "empire" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=zKsfi7Hy}Ducissa") 
                            : new TextObject("{=k8mH3vLE}{MALE}Dux{?}Queen{\\?}");

                        break;
                    }
                    case "empire" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=goLkPqMA}Cometessa") 
                            : new TextObject("{=JDn4mJFk}{MALE}Conte{?}Queen{\\?}");

                        break;
                    }
                    case "empire" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=srp9sKVz}Baronessa") 
                            : new TextObject("{=J3RPKk9n}{MALE}Baro{?}Queen{\\?}");

                        break;
                    }
                    case "empire" when female:
                        title = new TextObject("{=VPOx2yZJ}Domina");
                        break;
                    case "empire":
                        title = new TextObject("{=7sXfVNMb}{MALE}Dominus{?}Queen{\\?}");
                        break;
                    case "aserai" when type == TitleType.Kingdom:
                    {
                        title = female 
                            ? new TextObject("{=2BZMFLWX}Sultana") 
                            : new TextObject("{=SVbWZ70i}{MALE}Sultan{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when type == TitleType.Dukedom:
                    {
                        title = female
                            ? new TextObject("{=E9jX5Ts4}Emira") 
                            : new TextObject("{=RgCAx0d6}{MALE}Emir{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when type == TitleType.County:
                    {
                        title = female
                            ? new TextObject("{=rzWJMygK}Shaykah") 
                            : new TextObject("{=20fgXDt9}{MALE}Sheikh{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when type == TitleType.Barony:
                    {
                        title = female
                            ? new TextObject("{=1ybVoygi}Walia") 
                            : new TextObject("{=Te4jJVkb}{MALE}Wali{?}Queen{\\?}");

                        break;
                    }
                    case "aserai" when female:
                        title = new TextObject("{=1fpkmrWR}Beghum");
                        break;
                    case "aserai":
                        title = new TextObject("{=caeEyMhO}{MALE}Mawlaa{?}Queen{\\?}");
                        break;
                    case "khuzait" when type == TitleType.Kingdom:
                    {
                        title = female 
                            ? new TextObject("{=yDJ8ZJT2}Khatun") 
                            : new TextObject("{=Tx0r37Ah}{MALE}Khagan{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=QDp2ZJYL}Bekhi") 
                            : new TextObject("{=bK2NTbvH}{MALE}Baghatur{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=7rvFMmCt}Khanum") 
                            : new TextObject("{=NwRy1qXf}{MALE}Khan{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=a7yugqzq}Begum") 
                            : new TextObject("{=q8gPQFj1}{MALE}Bey{?}Queen{\\?}");

                        break;
                    }
                    case "khuzait" when female:
                        title = new TextObject("{=CsuNN5Wo}Khatagtai");
                        break;
                    case "khuzait":
                        title = new TextObject("{=B5qn1jUw}{MALE}Erxem{?}Queen{\\?}");
                        break;
                    case "sturgia" when type == TitleType.Kingdom:
                    {
                        title = female 
                            ? new TextObject("{=xXxWoAkS}Velikaya Knyaginya") 
                            : new TextObject("{=2FaNFxdG}{MALE}Velikiy Knyaz{?}Queen{\\?}");

                        break;
                    }
                    case "sturgia" when type == TitleType.Dukedom:
                    {
                        title = female 
                            ? new TextObject("{=NavJ6LSe}Knyaginya") 
                            : new TextObject("{=kKtkAMzF}{MALE}Knyaz{?}Queen{\\?}");

                        break;
                    }
                    case "sturgia" when type == TitleType.County:
                    {
                        title = female 
                            ? new TextObject("{=pvc2TNNC}Boyarina") 
                            : new TextObject("{=GbVGfJiO}{MALE}Boyar{?}Queen{\\?}");

                        break;
                    }
                    case "sturgia" when type == TitleType.Barony:
                    {
                        title = female 
                            ? new TextObject("{=ciaazLzm}Voivodina") 
                            : new TextObject("{=ESwdRARq}{MALE}Voivode{?}Queen{\\?}");

                        break;
                    }
                    case "sturgia" when female:
                        title = new TextObject("{=RUiVVkx9}Gospoda");
                        break;
                    case "sturgia":
                        title = new TextObject("{=NtBe0M9c}{MALE}Gospodin{?}Queen{\\?}");
                        break;
                }
            }

            if (title != null)
            {
                return title.ToString();
            }

            switch (type)
            {
                case TitleType.Kingdom when female:
                    title = new TextObject("{=meJbwYRd}Queen");
                    break;
                case TitleType.Kingdom:
                    title = new TextObject("{=50xBbuvG}{MALE}King{?}Queen{\\?}");
                    break;
                case TitleType.Dukedom when female:
                    title = new TextObject("{=Lp0LjO31}Duchess");
                    break;
                case TitleType.Dukedom:
                    title = new TextObject("{=vaNFKVw3}{MALE}Duke{?}Duchess{\\?}");
                    break;
                case TitleType.County when female:
                    title = new TextObject("{=ST2x2042}Countess");
                    break;
                case TitleType.County:
                    title = new TextObject("{=DFZeywid}{MALE}Count{?}Countess{\\?}");
                    break;
                case TitleType.Barony when female:
                    title = new TextObject("{=84UMVz2Y}Baroness");
                    break;
                case TitleType.Barony:
                    title = new TextObject("{=mRqVMLUk}{MALE}Baron{?}Baroness{\\?}");
                    break;
                case TitleType.Empire:
                case TitleType.Lordship:
                default:
                {
                    title = female 
                        ? new TextObject("{=D3tH28PU}Lady")
                        : new TextObject("{=u7GeyYD9}{MALE}Lord{?}Lady{\\?}");

                    break;
                }
            }

            return title.ToString();
        }

        public static string GetGovernmentString(GovernmentType type, CultureObject culture = null)
        {
            TextObject title = null;

            if (culture is {StringId: "sturgia"})
            {
                if (type == GovernmentType.Tribal)
                {
                    title = new TextObject("{=1uHPAkhc}Grand-Principality");
                }
            }

            if (title == null)
            {
                title = type switch
                {
                    GovernmentType.Feudal => new TextObject("{=beLkKir2}Kingdom"),
                    GovernmentType.Tribal => new TextObject("{=wza4DamA}High Kingship"),
                    GovernmentType.Imperial => new TextObject("{=q9bXtYjN}Empire"),
                    _ => new TextObject("{=OrecDEjd}Republic")
                };
            }

            return title.ToString();
        }

        public static string GetTitlePrefix(TitleType type, GovernmentType government, CultureObject culture = null)
        {
            TextObject title = null;

            if (culture != null)
            {
                switch (culture.StringId)
                {
                    case "sturgia" when type == TitleType.Kingdom:
                        title = new TextObject("{=1uHPAkhc}Grand-Principality");
                        break;
                    case "sturgia" when type == TitleType.Dukedom:
                        title = new TextObject("{=VQrYxoLM}Principality");
                        break;
                    case "sturgia" when type == TitleType.County:
                        title = new TextObject("{=ROt87J5a}Boyardom");
                        break;
                    case "sturgia" when type == TitleType.Barony:
                        title = new TextObject("{=HGjBKLuC}Voivodeship");
                        break;
                    case "sturgia":
                        title = new TextObject("{=48XdEOzr}Gospodin");
                        break;
                    case "aserai" when type == TitleType.Kingdom:
                        title = new TextObject("{=2BZMFLWX}Sultanate");
                        break;
                    case "aserai" when type == TitleType.Dukedom:
                        title = new TextObject("{=E9jX5Ts4}Emirate");
                        break;
                    case "aserai":
                    {
                        if (type == TitleType.County)
                        {
                            title = new TextObject("{=vDU6fhUL}Sheikhdom");
                        }

                        break;
                    }
                    case "battania":
                    {
                        if (government == GovernmentType.Tribal)
                        {
                            title = type switch
                            {
                                TitleType.Kingdom => new TextObject("{=godyRsGD}High-Kingdom"),
                                TitleType.Dukedom => new TextObject("{=ZBtAkJr4}Petty Kingdom"),
                                _ => title
                            };
                        }

                        break;
                    }
                }
            }

            title ??= type switch
            {
                TitleType.Kingdom => new TextObject("{=beLkKir2}Kingdom"),
                TitleType.Dukedom => new TextObject("{=4hM4EPXU}Dukedom"),
                TitleType.County => new TextObject("{=tTsB42Ls}County"),
                TitleType.Barony => new TextObject("{=oXRFuYuH}Barony"),
                _ => new TextObject("{=HCFo2Pdn}Lordship")
            };


            return title.ToString();
        }

        public static bool IsRetinueTroop(CharacterObject character)
        {
            var nobleRecruit = character.Culture.EliteBasicTroop;
            if (nobleRecruit.UpgradeTargets == null)
            {
                return false;
            }

            if (character == nobleRecruit)
            {
                return true;
            }

            if (nobleRecruit.UpgradeTargets != null)
            {
                var currentUpgrades = nobleRecruit.UpgradeTargets;
                while (currentUpgrades != null && currentUpgrades.Any())
                {
                    var upgrade = currentUpgrades[0];
                    if (upgrade == character)
                    {
                        return true;
                    }

                    currentUpgrades = upgrade.UpgradeTargets;
                }
            }

            return false;
        }

        public static bool IsRetinueTroop(CharacterObject character, CultureObject settlementCulture)
        {
            var culture = character.Culture;
            var nobleRecruit = culture.EliteBasicTroop;

            if (nobleRecruit.UpgradeTargets == null)
            {
                return false;
            }


            if (culture == settlementCulture)
            {
                if (character == nobleRecruit || nobleRecruit.UpgradeTargets.Contains(character))
                {
                    return true;
                }
            }

            return false;
        }

        public static CultureObject GetCulture(string id)
        {
            var culture = MBObjectManager.Instance.GetObjectTypeList<CultureObject>().FirstOrDefault(x => x.StringId == id);
            return culture;
        }

        public static ConsumptionType GetTradeGoodConsumptionType(ItemCategory item)
        {
            var id = item.StringId;
            if (item.Properties == Property.BonusToFoodStores)
            {
                return ConsumptionType.Food;
            }

            if (id is "silver" or "jewelry" or "spice" or "velvet" or "war_horse" ||
                id.EndsWith("4") || id.EndsWith("5"))
            {
                return ConsumptionType.Luxury;
            }

            if (id is "wool" or "pottery" or "cotton" or "flax" or "linen" or "leather" or "tools" 
                || id.EndsWith("3") || id.Contains("horse"))
            {
                return ConsumptionType.Industrial;
            }

            return ConsumptionType.General;
        }

        public static ConsumptionType GetTradeGoodConsumptionType(ItemObject item)
        {
            var id = item.StringId;
            switch (id)
            {
                case "silver" or "jewelry" or "spice" or "velvet" or "fur":
                    return ConsumptionType.Luxury;
                case "wool" or "pottery" or "cotton" or "flax" or "linen" or "leather" or "tools":
                    return ConsumptionType.Industrial;
            }

            if (item.IsFood)
            {
                return ConsumptionType.Food;
            }

            if (item.IsInitialized && !item.IsBannerItem &&
                (item.HasArmorComponent || item.HasWeaponComponent || item.IsAnimal ||
                 item.IsTradeGood || item.HasHorseComponent) && item.StringId != "undefined")
            {
                return ConsumptionType.General;
            }

            return ConsumptionType.None;
        }

        public static XmlDocument CreateDocumentFromXmlFile(string xmlPath)
        {
            var xmlDocument = new XmlDocument();
            var streamReader = new StreamReader(xmlPath);
            var xml = streamReader.ReadToEnd();
            xmlDocument.LoadXml(xml);
            streamReader.Close();
            return xmlDocument;
        }
    }
}