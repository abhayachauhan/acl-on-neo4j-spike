using System;
using System.Collections.Generic;
using Neo4j.Driver.V1;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var authToken = AuthTokens.Basic("neo4j", "password");
            using (var driver = GraphDatabase.Driver("bolt://192.168.99.100:7687", authToken))
            using (var session = driver.Session(AccessMode.Write))
            {
                DeleteAllNodes(session);

                SetupEmployees(session);
                SetupTeams(session);
                SetupGroups(session);
                SetupOnboardingForm(session);
                SetupPermissions(session);
                // DeleteAllNodes(session);

                FormsWhichEmployeeHasAccessTo(session, "emp2");
                WhoAccessAccessToForm(session, "A");
                WhatAccessDoesEmployeeHaveToForm(session, "A", "emp2");
            }
        }

        private static void WhatAccessDoesEmployeeHaveToForm(ISession session, string formInstanceId, string employeeId)
        {
            // WIP
            // string statement =
            //     "MATCH (emp:Employee {employeeid:{employeeid}}) " +
            //     "MATCH (team:Team)<--(emp) " +
            //     "MATCH (group:Group)<--(emp) " +
            //     "MATCH (form:FormInstance {forminstanceid:{forminstanceid}}) " +
            //     "MATCH (emp)-[perme]-(form) " +
            //     "MATCH (team)-[permt]-(form) " +
            //     "MATCH (group)-[permg]-(form) " +
            //     "RETURN perme AS perme, permt AS permt, permg AS permg";

            // var values = new Dictionary<string, object>() {
            //     {"employeeid", employeeId},
            //     {"forminstanceid", formInstanceId}
            // };

            // var result = session.Run(statement, values);

            // Console.WriteLine($"Employee {employeeId} access this access to form {formInstanceId}:  ");
            // foreach (var record in result)
            // {
            //     Console.WriteLine("FOUND");
            //     Console.WriteLine($"FormInstanceId: {record["perme"].As<string>()} {record["permt"].As<string>()} {record["permg"].As<string>()}");
            // }
        }

        private static void FormsWhichEmployeeHasAccessTo(ISession session, string employeeId)
        {
            string statement =
                "MATCH (emp:Employee {employeeid:{employeeid}}) " +
                "MATCH (emp)-[:READ]->(form:FormInstance) " +
                "RETURN form.formInstanceId AS formInstanceId " +
                "UNION " +
                "MATCH (emp:Employee {employeeid:{employeeid}})-[:PARTOF]->(group:Group) " +
                "MATCH (group)-[:READ]->(form:FormInstance) " +
                "RETURN form.formInstanceId AS formInstanceId " +
                "UNION " +
                "MATCH (emp:Employee {employeeid:{employeeid}})-[:PARTOF]->(team:Team) " +
                "MATCH (team)-[:READ]->(form:FormInstance) " +
                "RETURN form.formInstanceId AS formInstanceId ";

            var values = new Dictionary<string, object>() {
                {"employeeid", employeeId}
            };

            var result = session.Run(statement, values);

            Console.WriteLine($"Employee {values["employeeid"]} has access to: ");
            foreach (var record in result)
            {
                Console.WriteLine($"FormInstanceId: {record["formInstanceId"].As<string>()}");
            }
        }

        private static void WhoAccessAccessToForm(ISession session, string formInstanceId)
        {
            string statement = "MATCH (form:FormInstance {formInstanceId:\"A\"}) " +
                               "MATCH (employee:Employee)-->(form) " +
                               "MATCH (team:Team)-->(form) " +
                               "MATCH (group:Group)-->(form) " +
                               "RETURN employee.employeeid AS employee, team.teamid AS team, group.groupid AS group";
            var result = session.Run(statement, 
                new Dictionary<string, object>() { { "formInstanceId", formInstanceId } });

            Console.WriteLine($"Access to Form A is given to: ");

            foreach (var record in result)
            {
                Console.WriteLine($"Employee: {record["employee"].As<string>()}");
                Console.WriteLine($"Team: {record["team"].As<string>()}");
                Console.WriteLine($"Group: {record["group"].As<string>()}");
            }

        }

        private static void SetupPermissions(ISession session)
        {
            string statement = "MATCH (form:FormInstance {formInstanceId:\"A\"}) " +
                               "MATCH (employee:Employee {employeeid:\"emp1\"}) " +
                               "CREATE (employee)-[write:WRITE]->(form) " +
                               "CREATE (employee)-[read:READ]->(form)";
            session.Run(statement);

            statement = "MATCH (form:FormInstance {formInstanceId:\"B\"}) " +
                               "MATCH (employee:Employee {employeeid:\"emp2\"}) " +
                               "CREATE (employee)-[read:READ]->(form)";
            session.Run(statement);

            statement = "MATCH (form:FormInstance {formInstanceId:\"C\"}) " +
                               "MATCH (team:Team {teamid:\"team3\"}) " +
                               "CREATE (team)-[read:READ]->(form)";
            session.Run(statement);

            statement = "MATCH (form:FormInstance {formInstanceId:\"A\"}) " +
                               "MATCH (group:Group {groupid:\"group2\"}) " +
                               "CREATE (group)-[read:READ]->(form)";
            session.Run(statement);

            statement = "MATCH (form:FormInstance {formInstanceId:\"A\"}) " +
                               "MATCH (team:Team {teamid:\"team2\"}) " +
                               "CREATE (team)-[read:READ]->(form)";
            session.Run(statement);
        }

        private static void SetupGroups(ISession session)
        {
            string statement = "MATCH (e1:Employee {employeeid:\"emp2\"}) " +
                               "CREATE (e1)-[:PARTOF]->(:Group {groupid:\"group2\"})";
            session.Run(statement);
        }

        private static void SetupTeams(ISession session)
        {
            string statement = "MATCH (e1:Employee {employeeid:\"emp1\"}) " +
                               "CREATE (e1)-[partof:PARTOF]->(team:Team {teamid:\"team1\"})";
            session.Run(statement);

            statement = "MATCH (e2:Employee {employeeid:\"emp2\"}) " +
                               "CREATE (e2)-[partof:PARTOF]->(team:Team {teamid:\"team2\"})";
            session.Run(statement);

            statement = "MATCH (e3:Employee {employeeid:\"emp3\"}) " +
                               "CREATE (e3)-[partof:PARTOF]->(team:Team {teamid:\"team3\"})";
            session.Run(statement);
        }

        private static void SetupOnboardingForm(ISession session)
        {
            string statement = "FOREACH (formInstanceId in [\"A\",\"B\",\"C\"] | " +
                               "CREATE (:FormInstance {formInstanceId:formInstanceId}))";
            session.Run(statement);
        }

        private static void SetupEmployees(ISession session)
        {
            string statement = "FOREACH (employeeId in [\"emp1\",\"emp2\",\"emp3\"] | " +
                               "CREATE (:Employee {employeeid:employeeId}))";
            session.Run(statement);
        }

        private static void DeleteAllNodes(ISession session)
        {
            string statement = "MATCH (n) DETACH DELETE n";
            session.Run(statement);
        }
    }
}
