using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Koban
{
    public static class KioskExperiences
    {
        private static readonly Type[] expTypes;

        static KioskExperiences()
        {
            expTypes = typeof(KioskExperiences).GetTypeInfo().Assembly.GetTypes().Where(t =>
                t.Namespace != null && t.Namespace.StartsWith("Koban.Views")
                && t.GetTypeInfo().GetCustomAttribute<KioskExperienceAttribute>() != null)
                .ToArray();
        }

        public static IEnumerable<KioskExperience> Experiences
        {
            get
            {
                return expTypes.Select(t => new KioskExperience()
                {
                    PageType = t,
                    Attributes = t.GetTypeInfo().GetCustomAttribute<KioskExperienceAttribute>()
                }).OrderBy(e => e.Attributes.Title);
            }
        }
    }

    public class KioskExperience
    {
        public Type PageType { get; set; }
        public KioskExperienceAttribute Attributes { get; set; }
    }

    public enum ExperienceType
    {
        Kiosk,
        Other
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class KioskExperienceAttribute : Attribute
    {
        public string Title { get; set; }
        public string ImagePath { get; set; }
        public ExperienceType ExperienceType { get; set; }
    }
}
