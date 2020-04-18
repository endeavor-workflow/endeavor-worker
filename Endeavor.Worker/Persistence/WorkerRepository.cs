using Endeavor.Steps;
using System;
using System.Collections.Generic;
using System.Text;
using Keryhe.Persistence;
using System.Data;

namespace Endeavor.Worker.Persistence
{
    public class WorkerRepository : IRepository
    {
        private readonly IConnectionFactory _factory;
        public WorkerRepository(IConnectionFactory factory)
        {
            _factory = factory;
        }

        public Dictionary<string, object> GetStep(int stepId, string stepType)
        {
            using (IDbConnection conn = _factory.CreateConnection())
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@StepId", stepId }
                };

                var steps = conn.ExecuteQuery("Get" + stepType, CommandType.StoredProcedure, parameters);

                Dictionary<string, object> results = new Dictionary<string, object>();
                if(steps.Count == 1)
                {
                    results = steps[0];
                }
                else
                {
                    throw new Exception("Too many results returned.");
                }

                return results;
            }
        }

        public string GetTaskData(long taskId)
        {
            StringBuilder query = new StringBuilder();
            query.Append("SELECT TaskData FROM Task WHERE ID = ");
            query.Append(taskId.ToString());

            using (IDbConnection conn = _factory.CreateConnection())
            {
                conn.Open();
                var tasks = conn.ExecuteQuery(query.ToString(), CommandType.Text);

                string result = "";
                if (tasks.Count == 1)
                {
                    result = tasks[0]["TaskData"]?.ToString();
                }
                else
                {
                    throw new Exception("Too many results returned.");
                }

                return result;
            }
        }

        public void ReleaseTask(long taskId, string releaseValue, string output)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@TaskID", taskId },
                { "@ReleaseValue", releaseValue },
                { "@TaskData", output }
            };

            using (IDbConnection conn = _factory.CreateConnection())
            {
                conn.Open();
                conn.ExecuteNonQuery("ReleaseTask", CommandType.StoredProcedure, parameters);
            }
        }

        public void UpdateTaskStatus(long taskId, StatusType status)
        {
            int statusValue = (int)status;

            StringBuilder query = new StringBuilder();
            query.Append("UPDATE Task SET StatusValue = ");
            query.Append(statusValue.ToString());
            query.Append(" WHERE ID = ");
            query.Append(taskId.ToString());

            using (IDbConnection conn = _factory.CreateConnection())
            {
                conn.Open();
                conn.ExecuteNonQuery(query.ToString(), CommandType.Text);
            }
        }
    }
}
