using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Services.Batches.Interrogation.Models.Builders
{
    public class IssueBuilder
    {
        private readonly Issue _issue = new Issue();

        public IssueBuilder SetDescription(string description)
        {
            _issue.Description = description;
            return this;
        }

        public IssueBuilder SetRemediation(string remediation)
        {
            _issue.Remediation = remediation;
            return this;
        }

        public IssueBuilder SetMonitoringLevel(MonitoringLevel monitoringLevel)
        {
            _issue.MonitoringLevel = monitoringLevel;
            return this;
        }

        public IssueBuilder SetShouldContinueCheckingIssues(bool shouldContinueCheckingIssues)
        {
            _issue.ShouldContinueCheckingIssues = shouldContinueCheckingIssues;
            return this;
        }

        public Issue Build()
        {
            return _issue;
        }

        public void BuildAndAdd(Issues issues)
        {
            issues.Add(_issue);
        }
    }
}