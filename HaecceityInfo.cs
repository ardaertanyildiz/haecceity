using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Haecceity
{
    public class HaecceityInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Haecceity";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("f79ba8c7-eb7a-4100-aab7-b4c07b263ac8");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "ardae";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
