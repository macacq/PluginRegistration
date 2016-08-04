// =====================================================================
//
//  This file is part of the Microsoft Dynamics CRM SDK code samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
//
// =====================================================================

namespace Xrm.Sdk.PluginRegistration.Controls
{
    using Forms;
    using Helpers;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using Wrappers;

    [Designer(typeof(DocumentDesigner), typeof(IRootDesigner))]
    public partial class CrmAttributeSelectionControl : UserControl
    {
        private bool m_allAttributes;
        private Collection<string> m_attributeList = new Collection<string>();
        private CrmOrganization m_org;
        private string m_entityName;

        public event EventHandler<EventArgs> AttributesChanged;

        public CrmAttributeSelectionControl()
        {
            InitializeComponent();
        }

        [Browsable(false)]
        public CrmOrganization Organization
        {
            get
            {
                return m_org;
            }
            set
            {
                m_org = value;
                btnSelect.Enabled = (m_org != null && m_entityName != null);
            }
        }

        [Browsable(true)]
        public bool WordWrap
        {
            get
            {
                return txtAttributes.WordWrap;
            }

            set
            {
                txtAttributes.WordWrap = value;
            }
        }

        [Browsable(true)]
        public ScrollBars ScrollBars
        {
            get
            {
                return txtAttributes.ScrollBars;
            }

            set
            {
                txtAttributes.ScrollBars = value;
            }
        }

        [Browsable(true)]
        public string EntityName
        {
            get
            {
                return m_entityName;
            }
            set
            {
                if (!string.Equals(m_entityName, value, StringComparison.CurrentCulture))
                {
                    m_entityName = value;
                    btnSelect.Enabled = (m_org != null && m_entityName != null);
                }
            }
        }

        [Browsable(true)]
        public string Attributes
        {
            get
            {
                if (m_allAttributes)
                {
                    return null;
                }
                else
                {
                    return AttributeString;
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    m_allAttributes = true;
                    m_attributeList.Clear();

                    txtAttributes.Text = "All Attributes";
                }
                else
                {
                    m_allAttributes = false;
                    AttributeString = value;

                    txtAttributes.Text = string.Join(", ", AttributeCollectionToArray());
                }
            }
        }

        [Browsable(false)]
        public bool AllAttributes
        {
            get
            {
                return m_allAttributes;
            }
        }

        [Browsable(false)]
        public bool HasAttributes
        {
            get
            {
                return (m_allAttributes || m_attributeList.Count != 0);
            }
        }

        /// <summary>
        /// Shows this message when the control is disabled
        /// </summary>
        [Browsable(true)]
        public string DisabledMessage
        {
            get
            {
                return txtDisabledMessage.Text;
            }

            set
            {
                txtDisabledMessage.Text = value;
                DisplayDisabledMessage();
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (!Organization.IsEntityAttributesLoaded(EntityName))
            {
                WebServiceProgressForm progForm = new WebServiceProgressForm(this);
                OrganizationHelper.LoadAttributeList(Organization, EntityName, progForm.ProgressIndicator);

                if (Organization.AttributeLoadException != null)
                {
                    ErrorMessageForm.ShowErrorMessageBox(this, "Unable to load attribute list", "Attribute List Error",
                        Organization.AttributeLoadException);
                    return;
                }
            }

            AttributeSelectionForm selectorForm = new AttributeSelectionForm(UpdateParameters, m_org,
                Organization.RetrieveEntityAttributes(EntityName), m_attributeList, m_allAttributes);
            selectorForm.ShowDialog();
        }

        private void CrmAttributeSelectionControl_EnabledChanged(object sender, EventArgs e)
        {
            DisplayDisabledMessage();
        }

        #region Public Helper Methods

        public void ClearAttributes()
        {
            UpdateParameters(null, false);
        }

        #endregion Public Helper Methods

        #region Private Helper Methods

        private string AttributeString
        {
            get
            {
                if (m_attributeList.Count == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return string.Join(",", AttributeCollectionToArray());
                }
            }
            set
            {
                m_attributeList.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    string[] attributeList = value.Split(',');
                    foreach (string attribute in attributeList)
                    {
                        if (!string.IsNullOrEmpty(attribute))
                        {
                            m_attributeList.Add(attribute.Trim());
                        }
                    }
                }
            }
        }

        private string[] AttributeCollectionToArray()
        {
            return AttributeCollectionToArray(m_attributeList);
        }

        private string[] AttributeCollectionToArray(Collection<string> attributes)
        {
            if (attributes == null)
            {
                return new string[0];
            }
            else
            {
                string[] attributeList = new string[attributes.Count];
                attributes.CopyTo(attributeList, 0);

                return attributeList;
            }
        }

        private void UpdateParameters(Collection<string> attributes, bool allAttributes)
        {
            string newText;
            if (allAttributes)
            {
                newText = "All Attributes";
                m_allAttributes = true;
            }
            else
            {
                newText = string.Join(", ", AttributeCollectionToArray(attributes));
                m_allAttributes = false;
            }

            if (allAttributes != AllAttributes || !string.Equals(txtAttributes.Text, newText, StringComparison.CurrentCulture))
            {
                txtAttributes.Text = newText;
                m_allAttributes = allAttributes;

                m_attributeList.Clear();
                if (attributes != null && attributes.Count != 0)
                {
                    foreach (string attribute in attributes)
                    {
                        m_attributeList.Add(attribute);
                    }
                }

                if (AttributesChanged != null)
                {
                    AttributesChanged(this, new EventArgs());
                }
            }
        }

        private void DisplayDisabledMessage()
        {
            bool visibleDisabledMessage = !(Enabled || txtDisabledMessage.TextLength == 0);

            txtDisabledMessage.Visible = visibleDisabledMessage;
            txtAttributes.Visible = !visibleDisabledMessage;
            btnSelect.Visible = !visibleDisabledMessage;
        }

        #endregion Private Helper Methods
    }
}