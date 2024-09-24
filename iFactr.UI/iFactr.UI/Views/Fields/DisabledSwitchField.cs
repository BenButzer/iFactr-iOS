using MonoCross.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iFactr.Core.Forms
{
    public class DisabledSwitchField : Field
    {
        #region Properties
        /// <summary>
        /// Gets or sets the field value.
        /// </summary>
        public bool Value
        {
            get { return ParseValue(Text); }
            set { Text = value ? "On" : "Off"; }
        }
        #endregion

        #region Ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolField"/> class with no submission ID.
        /// </summary>
        public DisabledSwitchField() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolField"/> class using the name provided.
        /// </summary>
        /// <param name="id">A <see cref="String"/> representing the ID and Label values.</param>
        public DisabledSwitchField(string id) : this(id, id) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolField"/> class using the name provided.
        /// </summary>
        /// <param name="id">A <see cref="String"/> representing the ID.</param>
        /// /// <param name="label">A <see cref="String"/> representing the label value.</param>
        public DisabledSwitchField(string id, string label)
        {
            ID = id;
            Label = label;
        }

        #endregion

        /// <summary>
        /// Creates a deep-copy clone of this instance.
        /// </summary>
        public new DisabledSwitchField Clone()
        {
            return (DisabledSwitchField)CloneOverride();
        }

        /// <summary>
        /// Parses the specified string value into a boolean.
        /// </summary>
        /// <param name="val">The string value to convert.</param>
        /// <returns><c>true</c> if the string equates to "on", "true", or "yes"; otherwise <c>false</c>.</returns>
        public static bool ParseValue(string val)
        {
            return val != null && val.TryParseBoolean();
        }
    }
}