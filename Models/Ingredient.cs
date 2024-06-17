using Ruddy.WEB.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class Ingredient : IBaseEntity
    {
        public int Id { get; set; }
        public string NameFr { get; set; }
        public string NameEng { get; set; }
        public string NameEs { get; set; }
        public string NameNl { get; set; }
        public string Group { get; set; }
        public string SubGroup { get; set; }
        public double? Energy_KCal { get; set; }
        public double? Energy_Kj { get; set; }
        public double? Proteins_G { get; set; }
        public double? Fat_G { get; set; }
        public double? Carbohydrates_G { get; set; }
        public double? Sugars_G { get; set; }
        public double? Starch_G { get; set; }
        public double? Water_G { get; set; }
        public double? FattyAcidsSaturated_G { get; set; }
        public double? Glucose_G { get; set; }
        public double? Lactose_G { get; set; }
        public double? Fructose_G { get; set; }
        public double? Sucrose_G { get; set; }
        public double? Maltose_G { get; set; }
        public double? GaLactose_G { get; set; }
        public double? FibresSum_G { get; set; }
        public double? FA_C12_0_G { get; set; }
        public double? FA_C14_0_G { get; set; }
        public double? FA_C16_0_G { get; set; }
        public double? FA_monounsat_sum_G { get; set; }
        public double? FA_polyunsat_sum_G { get; set; }
        public double? FA_omega_3_sum_G { get; set; }
        public double? FA_C20_5_n_3_cis_EPA_G { get; set; }
        public double? FA_C22_6_n_3_cis_DHA_G { get; set; }
        public double? FA_C18_3_n_3_cis_linolenic_G { get; set; }
        public double? FA_omega_6_sum_G { get; set; }
        public double? FA_C18_2_n_6_cis_linoleic_G { get; set; }
        public double? FA_trans_sum_G { get; set; }
        public double? Cholesterol_MG { get; set; }
        public double? Alcohol_G { get; set; }
        public double? Polyols_sum_G { get; set; }
        public double? Sodium_MG { get; set; }
        public double? Potassium_MG { get; set; }
        public double? Calcium_MG { get; set; }
        public double? Phosphorus_MG { get; set; }
        public double? Magnesium_MG { get; set; }
        public double? Iron_MG { get; set; }
        public double? Copper_MG { get; set; }
        public double? Zinc_MG { get; set; }
        public double? Iodide_MG { get; set; }
        public double? Selenium_MCG { get; set; }
        public double? VitA_Activity_MCG { get; set; }
        public double? VitB1_MG { get; set; }
        public double? VitB2_MG { get; set; }
        public double? VitB12_MCG { get; set; }
        public double? Folate_MCG { get; set; }
        public double? VitC_MG { get; set; }
        public double? VitD_MCG { get; set; }
        public double? VitE_MG { get; set; }
        //0 not inserted, 1 inserted just partly and should be removed, 2 inserted Completely 
        public int isChainInsertedByAdmin { get; set; } = 0;
        public int? CraftedComponentId { get; set; }
        public CraftedComponent CraftedComponent { get; set; }

    }
}
