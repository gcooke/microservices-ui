using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Gateway.Web.Models.Batches
{
    /// <summary>
    /// Data transfer object representing a request to create an echo. This is a duplicate representation of the official version in Bagl.CIB.Sigma.Models. The reason the
    /// the code is duplicated is because we do not want a reference to models in the Redstone dashboard at the moment because of further dependencies on QA.
    /// </summary>
    public class EchoCreationDto
    {
        /// <summary>
        /// Get or set the user who initiated this calculation.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Get or set the name of the platform that initiated the calculation (e.g. Excel, Sigma Viewer, Risk Batch Harness etc)
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Get or set the site (e.g. GHANA)
        /// </summary>
        public string Site { get; set; }

        /// <summary>
        /// Get or set the name of the calculation.
        /// </summary>
        public string CalculationName { get; set; }

        /// <summary>
        /// Get or set the start date and time of the calculation.
        /// </summary>
        public DateTime Started { get; set; }

        /// <summary>
        /// Get or set the valuation date of the calculation.
        /// </summary>
        public DateTime ValuationDate { get; set; }

        /// <summary>
        /// Get or set the serialized echo data for the calculation.
        /// </summary>
        public EchoParametersDto Parameters { get; set; }
    }

    public class EchoParametersDto
    {
        public EchoParametersDto()
        {
            Categories = new List<EchoCategoryDto>();
        }

        public List<EchoCategoryDto> Categories { get; set; }
    }

    public class EchoCategoryDto
    {
        public EchoCategoryDto()
        {
            Entries = new List<EchoEntryDto>();
        }

        public string Name { get; set; }
        public List<EchoEntryDto> Entries { get; set; }
    }

    public class EchoEntryDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string AdditionalData { get; set; }
    }
}