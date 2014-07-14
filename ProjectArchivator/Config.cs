using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ProjectArchivator
{
    public class ProjectArchivatorConfig : ConfigurationSection
    {
        [ConfigurationProperty("recepientCert", IsKey = true, IsRequired = true)]
        public string RecepientCert { get { return this["recepientCert"] as string; } }
        [ConfigurationProperty("sourceFolder", IsKey = true, IsRequired = true)]
        public string SourceFolder { get { return this["sourceFolder"] as string; } }
        [ConfigurationProperty("systemGuid", IsKey = true, IsRequired = true)]
        public string SystemGuid { get { return this["systemGuid"] as string; } }
        
        
        public static ProjectArchivatorConfig GetConfig()
        {
            return (ProjectArchivatorConfig)ConfigurationManager.GetSection("ProjectArchivatorConfig") ?? new ProjectArchivatorConfig();
        }
        [ConfigurationProperty("Enveroument")]
        public EnveroumentCfgClass Enveroument
        {
            get
            {
                return (EnveroumentCfgClass)this["Enveroument"];// ?? new ServerCfgClass();
            }
        }
        [ConfigurationProperty("ImgTypes")]
        public ImgTypesCfgSection ImgTypes
        {
            get
            {
                return (ImgTypesCfgSection)this["ImgTypes"];// ?? new ServerCfgClass();
            }
        }
        [ConfigurationProperty("FilePatterns")]
        public FilePatternsCfgSection FilePatterns
        {
            get
            {
                return (FilePatternsCfgSection)this["FilePatterns"];// ?? new ServerCfgClass();
            }
        }
    }
    public class EnveroumentCfgClass : ConfigurationElement
    {
        //[ConfigurationProperty("bindHost", IsKey = true, IsRequired = true)]
        //public string Host { get { return this["bindHost"] as string; } }
        //[ConfigurationProperty("bindPort", IsKey = true, IsRequired = true)]
        //public UInt16 Port { get { return (UInt16)this["bindPort"]; } }
        //[ConfigurationProperty("enquireLinkPeriod", IsKey = true, IsRequired = true)]
        //public UInt32 EnquireLinkPeriod { get { return (UInt32)this["enquireLinkPeriod"]; } }


        [ConfigurationProperty("Paths")]
        public PathsCfgSection Paths
        {
            get
            {
                return (PathsCfgSection)this["Paths"] ?? new PathsCfgSection();
            }
        }
        [ConfigurationProperty("ExcludeFolders")]
        public ExcludeFoldersCfgSection ExcludeFolders
        {
            get
            {
                return (ExcludeFoldersCfgSection)this["ExcludeFolders"] ?? new ExcludeFoldersCfgSection();
            }
        }
    }
    [ConfigurationCollection(typeof(PathCfgElement), AddItemName = "Path")]
    public class PathsCfgSection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PathCfgElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PathCfgElement)element).Name;
        }
        public PathCfgElement this[string index]
        {
            get
            {
                for (int i = 0; i < base.Count; i++)
                {
                    if (((PathCfgElement)base.BaseGet(i)).Name == index)
                    {
                        return (PathCfgElement)base.BaseGet(i);
                    }
                }
                return null;
            }
        }
    }
    [ConfigurationCollection(typeof(ImageTypeCfgElement), AddItemName = "ImgType")]
    public class ImgTypesCfgSection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ImageTypeCfgElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ImageTypeCfgElement)element).Name;
        }
        public ImageTypeCfgElement this[string index]
        {
            get
            {
                for (int i = 0; i < base.Count; i++)
                {
                    if (((ImageTypeCfgElement)base.BaseGet(i)).Name == index)
                    {
                        return (ImageTypeCfgElement)base.BaseGet(i);
                    }
                }
                return null;
            }
        }
    }
    [ConfigurationCollection(typeof(ExcludeFoldersCfgElement), AddItemName = "ExcludeFolder")]
    public class ExcludeFoldersCfgSection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ExcludeFoldersCfgElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExcludeFoldersCfgElement)element).Name;
        }
        public ExcludeFoldersCfgElement this[string index]
        {
            get
            {
                for (int i = 0; i < base.Count; i++)
                {
                    if (((ExcludeFoldersCfgElement)base.BaseGet(i)).Name == index)
                    {
                        return (ExcludeFoldersCfgElement)base.BaseGet(i);
                    }
                }
                return null;
            }
        }
    }
    [ConfigurationCollection(typeof(ExcludeFoldersCfgElement), AddItemName = "Pattern")]
    public class FilePatternsCfgSection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FilePatternCfgElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FilePatternCfgElement)element).Name;
        }
        public FilePatternCfgElement this[string index]
        {
            get
            {
                for (int i = 0; i < base.Count; i++)
                {
                    if (((FilePatternCfgElement)base.BaseGet(i)).Name == index)
                    {
                        return (FilePatternCfgElement)base.BaseGet(i);
                    }
                }
                return null;
            }
        }
    }
    public class PathCfgElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name { get { return this["name"] as string; } }
        [ConfigurationProperty("value", IsKey = true, IsRequired = true)]
        public string Value { get { return this["value"] as string; } }
    }
    public class ImageTypeCfgElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name { get { return this["name"] as string; } }
        [ConfigurationProperty("value", IsKey = true, IsRequired = true)]
        public string Value { get { return this["value"] as string; } }
    }
    public class ExcludeFoldersCfgElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name { get { return this["name"] as string; } }
        [ConfigurationProperty("value", IsKey = true, IsRequired = true)]
        public string Value { get { return this["value"] as string; } }
    }
    public class FilePatternCfgElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name { get { return this["name"] as string; } }
        [ConfigurationProperty("value", IsKey = true, IsRequired = true)]
        public string Value { get { return this["value"] as string; } }
    }
}
