using UiPath.CodedWorkflows;
using System;

namespace RPA_PHARMA_PRESCRIPTION_INTEGRATOR
{
    public class GoogleDocsFactory
    {
        public GoogleDocsFactory(ICodedWorkflowsServiceContainer resolver)
        {
        }
    }

    public class DriveFactory
    {
        public UiPath.GSuite.Activities.Api.DriveConnection My_Workspace_isabellebrandaoo44_gmail_com { get; set; }

        public DriveFactory(ICodedWorkflowsServiceContainer resolver)
        {
            My_Workspace_isabellebrandaoo44_gmail_com = new UiPath.GSuite.Activities.Api.DriveConnection("4585e710-4003-483f-8d60-8000a2c2fea5", resolver);
        }
    }

    public class GoogleFormsFactory
    {
        public GoogleFormsFactory(ICodedWorkflowsServiceContainer resolver)
        {
        }
    }

    public class GmailFactory
    {
        public GmailFactory(ICodedWorkflowsServiceContainer resolver)
        {
        }
    }

    public class GoogleSheetsFactory
    {
        public GoogleSheetsFactory(ICodedWorkflowsServiceContainer resolver)
        {
        }
    }

    public class GoogleTasksFactory
    {
        public GoogleTasksFactory(ICodedWorkflowsServiceContainer resolver)
        {
        }
    }

    public class GoogleWorkspaceFactory
    {
        public GoogleWorkspaceFactory(ICodedWorkflowsServiceContainer resolver)
        {
        }
    }
}