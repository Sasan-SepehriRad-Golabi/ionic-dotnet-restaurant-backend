using Ruddy.WEB.Enums;
using Ruddy.WEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.ViewModels
{
    public class StatisticsViewModel
    {
        public DateTime? BirthDate { get; set; }
        public Gender Gender { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public int LevelOfActivity { get; set; }
        public List<OrderСharacteristics> OrderСharacteristics { get; set; }
    }

    public class OrderСharacteristics
    {
        public DateTime OrderDate { get; set; }

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

        public void AddIngridient(ItemСharacteristics next, int count)
        {
            Energy_KCal += next.Energy_KCal * count;
            Energy_Kj += next.Energy_Kj * count;
            Proteins_G += next.Proteins_G * count;
            Fat_G += next.Fat_G * count;
            Carbohydrates_G += next.Carbohydrates_G * count;
            Sugars_G += next.Sugars_G * count;
            Starch_G += next.Starch_G * count;
            Water_G += next.Water_G * count;
            FattyAcidsSaturated_G += next.FattyAcidsSaturated_G * count;
            Glucose_G += next.Glucose_G * count;
            Lactose_G += next.Lactose_G * count;
            Fructose_G += next.Fructose_G * count;
            Sucrose_G += next.Sucrose_G * count;
            Maltose_G += next.Maltose_G * count;
            GaLactose_G += next.GaLactose_G * count;
            FibresSum_G += next.FibresSum_G * count;
            FA_C12_0_G += next.FA_C12_0_G * count;
            FA_C14_0_G += next.FA_C14_0_G * count;
            FA_C16_0_G += next.FA_C16_0_G * count;
            FA_monounsat_sum_G += next.FA_monounsat_sum_G * count;
            FA_polyunsat_sum_G += next.FA_polyunsat_sum_G * count;
            FA_omega_3_sum_G += next.FA_omega_3_sum_G * count;
            FA_C20_5_n_3_cis_EPA_G += next.FA_C20_5_n_3_cis_EPA_G * count;
            FA_C22_6_n_3_cis_DHA_G += next.FA_C22_6_n_3_cis_DHA_G * count;
            FA_C18_3_n_3_cis_linolenic_G += next.FA_C18_3_n_3_cis_linolenic_G * count;
            FA_omega_6_sum_G += next.FA_omega_6_sum_G * count;
            FA_C18_2_n_6_cis_linoleic_G += next.FA_C18_2_n_6_cis_linoleic_G * count;
            FA_trans_sum_G += next.FA_trans_sum_G * count;
            Cholesterol_MG += next.Cholesterol_MG * count;
            Alcohol_G += next.Alcohol_G * count;
            Polyols_sum_G += next.Polyols_sum_G * count;
            Sodium_MG += next.Sodium_MG * count;
            Potassium_MG += next.Potassium_MG * count;
            Calcium_MG += next.Calcium_MG * count;
            Phosphorus_MG += next.Phosphorus_MG * count;
            Magnesium_MG += next.Magnesium_MG * count;
            Iron_MG += next.Iron_MG * count;
            Copper_MG += next.Copper_MG * count;
            Zinc_MG += next.Zinc_MG * count;
            Iodide_MG += next.Iodide_MG * count;
            Selenium_MCG += next.Selenium_MCG * count;
            VitA_Activity_MCG += next.VitA_Activity_MCG * count;
            VitB1_MG += next.VitB1_MG * count;
            VitB2_MG += next.VitB2_MG * count;
            VitB12_MCG += next.VitB12_MCG * count;
            Folate_MCG += next.Folate_MCG * count;
            VitC_MG += next.VitC_MG * count;
            VitD_MCG += next.VitD_MCG * count;
            VitE_MG += next.VitE_MG * count;
        }
    }
}
