using System;
using System.Collections.Generic;
using System.Text;

namespace Ruddy.WEB.Models
{
    public class ItemСharacteristics
    {
        public int Id { get; set; }
        public double Energy_KCal { get; set; }
        public double Energy_Kj { get; set; }
        public double Proteins_G { get; set; }
        public double Fat_G { get; set; }
        public double Carbohydrates_G { get; set; }
        public double Sugars_G { get; set; }
        public double Starch_G { get; set; }
        public double Water_G { get; set; }
        public double FattyAcidsSaturated_G { get; set; }
        public double Glucose_G { get; set; }
        public double Lactose_G { get; set; }
        public double Fructose_G { get; set; }
        public double Sucrose_G { get; set; }
        public double Maltose_G { get; set; }
        public double GaLactose_G { get; set; }
        public double FibresSum_G { get; set; }
        public double FA_C12_0_G { get; set; }
        public double FA_C14_0_G { get; set; }
        public double FA_C16_0_G { get; set; }
        public double FA_monounsat_sum_G { get; set; }
        public double FA_polyunsat_sum_G { get; set; }
        public double FA_omega_3_sum_G { get; set; }
        public double FA_C20_5_n_3_cis_EPA_G { get; set; }
        public double FA_C22_6_n_3_cis_DHA_G { get; set; }
        public double FA_C18_3_n_3_cis_linolenic_G { get; set; }
        public double FA_omega_6_sum_G { get; set; }
        public double FA_C18_2_n_6_cis_linoleic_G { get; set; }
        public double FA_trans_sum_G { get; set; }
        public double Cholesterol_MG { get; set; }
        public double Alcohol_G { get; set; }
        public double Polyols_sum_G { get; set; }
        public double Sodium_MG { get; set; }
        public double Potassium_MG { get; set; }
        public double Calcium_MG { get; set; }
        public double Phosphorus_MG { get; set; }
        public double Magnesium_MG { get; set; }
        public double Iron_MG { get; set; }
        public double Copper_MG { get; set; }
        public double Zinc_MG { get; set; }
        public double Iodide_MG { get; set; }
        public double Selenium_MCG { get; set; }
        public double VitA_Activity_MCG { get; set; }
        public double VitB1_MG { get; set; }
        public double VitB2_MG { get; set; }
        public double VitB12_MCG { get; set; }
        public double Folate_MCG { get; set; }
        public double VitC_MG { get; set; }
        public double VitD_MCG { get; set; }
        public double VitE_MG { get; set; }

        public void AddIngridient(Ingredient ingredient, double weight)
        {
            var weightRate = weight / 100.0;

            Energy_KCal += (ingredient.Energy_KCal ?? 0.0) * weightRate;
            Energy_Kj += (ingredient.Energy_Kj ?? 0.0) * weightRate;
            Proteins_G += (ingredient.Proteins_G ?? 0.0) * weightRate;
            Fat_G += (ingredient.Fat_G ?? 0.0) * weightRate;
            Carbohydrates_G += (ingredient.Carbohydrates_G ?? 0.0) * weightRate;
            Sugars_G += (ingredient.Sugars_G ?? 0.0) * weightRate;
            Starch_G += (ingredient.Starch_G ?? 0.0) * weightRate;
            Water_G += (ingredient.Water_G ?? 0.0) * weightRate;
            FattyAcidsSaturated_G += (ingredient.FattyAcidsSaturated_G ?? 0.0) * weightRate;
            Glucose_G += (ingredient.Glucose_G ?? 0.0) * weightRate;
            Lactose_G += (ingredient.Lactose_G ?? 0.0) * weightRate;
            Fructose_G += (ingredient.Fructose_G ?? 0.0) * weightRate;
            Sucrose_G += (ingredient.Sucrose_G ?? 0.0) * weightRate;
            Maltose_G += (ingredient.Maltose_G ?? 0.0) * weightRate;
            GaLactose_G += (ingredient.GaLactose_G ?? 0.0) * weightRate;
            FibresSum_G += (ingredient.FibresSum_G ?? 0.0) * weightRate;
            FA_C12_0_G += (ingredient.FA_C12_0_G ?? 0.0) * weightRate;
            FA_C14_0_G += (ingredient.FA_C14_0_G ?? 0.0) * weightRate;
            FA_C16_0_G += (ingredient.FA_C16_0_G ?? 0.0) * weightRate;
            FA_monounsat_sum_G += (ingredient.FA_monounsat_sum_G ?? 0.0) * weightRate;
            FA_polyunsat_sum_G += (ingredient.FA_polyunsat_sum_G ?? 0.0) * weightRate;
            FA_omega_3_sum_G += (ingredient.FA_omega_3_sum_G ?? 0.0) * weightRate;
            FA_C20_5_n_3_cis_EPA_G += (ingredient.FA_C20_5_n_3_cis_EPA_G ?? 0.0) * weightRate;
            FA_C22_6_n_3_cis_DHA_G += (ingredient.FA_C22_6_n_3_cis_DHA_G ?? 0.0) * weightRate;
            FA_C18_3_n_3_cis_linolenic_G += (ingredient.FA_C18_3_n_3_cis_linolenic_G ?? 0.0) * weightRate;
            FA_omega_6_sum_G += (ingredient.FA_omega_6_sum_G ?? 0.0) * weightRate;
            FA_C18_2_n_6_cis_linoleic_G += (ingredient.FA_C18_2_n_6_cis_linoleic_G ?? 0.0) * weightRate;
            FA_trans_sum_G += (ingredient.FA_trans_sum_G ?? 0.0) * weightRate;
            Cholesterol_MG += (ingredient.Cholesterol_MG ?? 0.0) * weightRate;
            Alcohol_G += (ingredient.Alcohol_G ?? 0.0) * weightRate;
            Polyols_sum_G += (ingredient.Polyols_sum_G ?? 0.0) * weightRate;
            Sodium_MG += (ingredient.Sodium_MG ?? 0.0) * weightRate;
            Potassium_MG += (ingredient.Potassium_MG ?? 0.0) * weightRate;
            Calcium_MG += (ingredient.Calcium_MG ?? 0.0) * weightRate;
            Phosphorus_MG += (ingredient.Phosphorus_MG ?? 0.0) * weightRate;
            Magnesium_MG += (ingredient.Magnesium_MG ?? 0.0) * weightRate;
            Iron_MG += (ingredient.Iron_MG ?? 0.0) * weightRate;
            Copper_MG += (ingredient.Copper_MG ?? 0.0) * weightRate;
            Zinc_MG += (ingredient.Zinc_MG ?? 0.0) * weightRate;
            Iodide_MG += (ingredient.Iodide_MG ?? 0.0) * weightRate;
            Selenium_MCG += (ingredient.Selenium_MCG ?? 0.0) * weightRate;
            VitA_Activity_MCG += (ingredient.VitA_Activity_MCG ?? 0.0) * weightRate;
            VitB1_MG += (ingredient.VitB1_MG ?? 0.0) * weightRate;
            VitB2_MG += (ingredient.VitB2_MG ?? 0.0) * weightRate;
            VitB12_MCG += (ingredient.VitB12_MCG ?? 0.0) * weightRate;
            Folate_MCG += (ingredient.Folate_MCG ?? 0.0) * weightRate;
            VitC_MG += (ingredient.VitC_MG ?? 0.0) * weightRate;
            VitD_MCG += (ingredient.VitD_MCG ?? 0.0) * weightRate;
            VitE_MG += (ingredient.VitE_MG ?? 0.0) * weightRate;
        }

    }
}
