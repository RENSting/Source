using Cnf.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cnf.Project.Employee.Entity;
using Cnf.CodeBase.Serialize;

namespace TestLab.Employee
{
    class TestReferenceController
    {
        readonly WebConnector _connector;

        public TestReferenceController()
        {
            _connector = new WebConnector("https://localhost:5001");

        }

        internal async Task Start()
        {
            RightSpecialtyConvert();
            WrongSpecialtyConvert();
            RightQualificationConvert();
            WrongQualificationConvert();
            RightDutyConvert();
            WrongDutyConvert();

            //await InsertSpecialties();
            //await InsertQualifications();
            //await InsertDuties();
        }

        void RightSpecialtyConvert()
        {
            Reference reference = new Reference()
            {
                ActiveStatus = true,
                ReferenceCode = "S001",
                Type = ReferenceTypeEnum.Specialty,
                ReferenceValue = "焊接"
            };

            try
            {
                Specialty specialty = reference;
                Console.WriteLine("正确转换了参照项: 专业名称：" + specialty.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("转换参照项错误: 参照项：" + reference.ReferenceType);
            }
        }

        void WrongSpecialtyConvert()
        {
            Reference reference = new Reference()
            {
                ActiveStatus = true,
                ReferenceCode = "D001",
                Type = ReferenceTypeEnum.Duty,
                ReferenceValue = "安全经理"
            };

            try
            {
                Specialty specialty = reference;
                Console.WriteLine("错误转换了参照项: 专业名称：" + reference.ReferenceType + specialty.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("正确识别了参照项错误: 参照项：" + reference.ReferenceType + " " + ex.Message);
            }
        }

        void RightQualificationConvert()
        {
            Reference reference = new Reference()
            {
                ActiveStatus = true,
                ReferenceCode = "Q001",
                Type = ReferenceTypeEnum.Qualification,
                ReferenceValue = "一级建造师"
            };

            try
            {
                Qualification qualification = reference;
                Console.WriteLine("正确转换了参照项: 资格类型名称：" + qualification.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("转换参照项错误: 参照项：" + reference.ReferenceType);
            }
        }

        void WrongQualificationConvert()
        {
            Reference reference = new Reference()
            {
                ActiveStatus = true,
                ReferenceCode = "D001",
                Type = ReferenceTypeEnum.Duty,
                ReferenceValue = "安全经理"
            };

            try
            {
                Qualification qualification = reference;
                Console.WriteLine("错误转换了参照项: 资格类型名称：" + reference.ReferenceType + qualification.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("正确识别了参照项错误: 参照项：" + reference.ReferenceType + " " + ex.Message);
            }
        }

        void RightDutyConvert()
        {
            Reference reference = new Reference()
            {
                ActiveStatus = true,
                ReferenceCode = "D001",
                Type = ReferenceTypeEnum.Duty,
                ReferenceValue = "安全经理"
            };

            try
            {
                Duty duty = reference;
                Console.WriteLine("正确转换了参照项: 职责名称：" + duty.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("转换参照项错误: 参照项：" + reference.ReferenceType);
            }
        }

        void WrongDutyConvert()
        {
            Reference reference = new Reference()
            {
                ActiveStatus = true,
                ReferenceCode = "S001",
                Type = ReferenceTypeEnum.Specialty,
                ReferenceValue = "焊接"
            };

            try
            {
                Duty duty = reference;
                Console.WriteLine("错误转换了参照项: 职责名称：" + reference.ReferenceType + duty.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("正确识别了参照项错误: 参照项：" + reference.ReferenceType + " " + ex.Message);
            }
        }

        async Task InsertSpecialties()
        {
            var references = new List<Reference>
            {
                new Specialty("S001", "焊接", 1),
                new Specialty("S002", "管道", 1),
                new Specialty("S003", "机械", 1),
                new Specialty("S004", "电仪", 1),
            };

            await InsertReferences(references);
        }

        async Task InsertQualifications()
        {
            var references = new List<Reference>
            {
                new Qualification("Q001", "一级建造师", 1),
                new Qualification("Q002", "二级建造师", 1),
                new Qualification("Q003", "造价师", 1),
            };

            await InsertReferences(references);
        }

        async Task InsertDuties()
        {
            var references = new List<Reference>
            {
                new Duty("D001", "项目经理", 1),
                new Duty("D002", "技术经理", 1),
                new Duty("D003", "安全经理", 1),
                new Duty("D004", "商务经理", 1),
            };

            await InsertReferences(references);
        }

        async Task InsertReferences(List<Reference> references)
        {
            try
            {
                foreach(var reference in references)
                {
                    var result = await _connector.PostAsync<Reference, int>("api/Reference/Save", reference);
                    if(result.IsSuccess)
                    {
                        Console.WriteLine($"插入参照项成功, " +
                            $"类型:{reference.ReferenceType}, " +
                            $"代码:{reference.ReferenceCode}, " +
                            $"值:{reference.ReferenceValue}, " +
                            $"ID:{result.GetData()}");
                    }
                    else
                    {
                        Console.WriteLine($"插入参照项失败, 值:{reference.ReferenceValue}, 错误:{result.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"插入参照项发生操作异常, 错误:{ex.Message}");
            }
        }
    }
}
