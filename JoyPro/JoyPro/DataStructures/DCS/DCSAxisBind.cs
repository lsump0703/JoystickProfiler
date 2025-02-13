﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public class DCSAxisBind
    {
        public DCSAxisFilter filter;
        public string key;
        public string JPRelName;
        public List<string> Groups;
        public Bind relatedBind; //don't 

        public DCSAxisBind()
        {
            filter = new DCSAxisFilter();
            key = "";
            JPRelName = "";
            Groups = new List<string>();
        }
        public DCSAxisBind Copy()
        {
            DCSAxisBind result = new DCSAxisBind();
            result.JPRelName = JPRelName;
            result.key = key;
            result.filter = filter.Copy();
            result.Groups = new List<string>();
            for(int i=0; i<Groups.Count; ++i)
            {
                result.Groups.Add(Groups[i]);
            }
            return result;
        }


    }
}
