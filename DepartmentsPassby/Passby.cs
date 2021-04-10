using System;
using System.Collections.Generic;
using DepartmentsPassby.Models;

namespace DepartmentsPassby
{
    public class Passby
    {   
        private Department[] Departments { get; }

        private Dictionary<string, int> DepartmentsMapper { get; }

        private SealsManager SealsManager { get; } = new();

        public Passby(string configPath)
        {
            Departments = ConfigParser.Parse(configPath);
            DepartmentsMapper = CreateDepartmentsMapper(Departments);
        }

        public List<List<string>> Start(string targetDepartmentName)
        {
            Departments[DepartmentsMapper[targetDepartmentName]].IsTarget = true;
            
            for (int i = 0; i < Departments.Length; )
            {
                var department = Departments[i];
                var rule = department.Rule;

                switch (rule.Type)
                {
                    case "unconditional": 
                        i = CallAction(i, rule.Action, rule.Param);
                        break;
                    case "conditional":
                        if (SealsManager.Seals.Contains(rule.ConditionalPrint)) 
                            i = CallAction(i, rule.Action, rule.Param);
                        else
                            i = CallAction(i, rule.ElseAction, rule.ElseParam);
                        break;
                }

                string curtentSealsSum = SealsManager.GetSealsSum();
                if (IsInfiniteLoop(curtentSealsSum, department.SealsSum))
                {
                    Console.WriteLine("Infinite loop detected");
                    break;
                }
              
                department.SealsSum = curtentSealsSum;
                if (department.IsTarget) SealsManager.MakeSealsSnapshot();
            }

            return SealsManager.SealsSnapshots;
        }
        
        private int CallAction(int index, string actionName, string param )
        {
            switch (actionName)
            {
                case "put": 
                    SealsManager.PutSeal(param);
                    return ++index;
                case "delete": 
                    SealsManager.DeleteSeal(param);
                    return ++index;
                case "goto":
                    return DepartmentsMapper[param];
                default: return index;
            }
        }

        private bool IsInfiniteLoop(string currentSealsSum, string departmentSealsSum)
        {
            if (departmentSealsSum == null) return false;
            if (departmentSealsSum == currentSealsSum) return true;
            return false;
        }
        
        private Dictionary<string, int> CreateDepartmentsMapper(Department[] departments)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            for (int i = 0; i < departments.Length; i++)
            {
                result.Add(departments[i].Title, i);
            }

            return result;
        }
    }
}