﻿using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Restify3SP
{
    internal class EnrichedDynamicParameters : DynamicParameters
    {
        private List<ParameterInfo> exposedParameters;
        private static Regex parameterMatcher = new Regex("@([a-zA-Z0-9\\-]+) ?= ?('?[a-zA-Z0-9_@.<>!?äÄüÜöÖ\\-:]+'?)( out)?,?");
        public static EnrichedDynamicParameters FromArguments(string arguments)
        {
            EnrichedDynamicParameters parameters = new EnrichedDynamicParameters();
            if (string.IsNullOrEmpty(arguments))
            {
                return parameters;
            }
            var matches = parameterMatcher.Matches(arguments);
            matches.Where(m => m.Groups.Count >= 3).
                ToList().ForEach(
                    s => parameters.AddParameter(
                        s.Groups[1].Value, 
                        s.Groups[2].Value,
                        null,
                        s.Groups.Count > 3 && s.Groups[3].Value == " out" ? ParameterDirection.Output : ParameterDirection.Input,
                        null));
            return parameters;
        }
        public EnrichedDynamicParameters()
        {
            this.exposedParameters = new List<ParameterInfo>();
        }
        public void AddParameter(
            string name,
            object value,
            DbType? dbtype,
            ParameterDirection? direction,
            int? size)
        {
            this.exposedParameters.Add(
                new ParameterInfo() 
                { 
                    Name = name,
                    Value = value,
                    DbType = dbtype,
                    Direction = direction,
                    Size = size
                });
            this.Add(name, value, dbtype, direction, size);
        }
        public Dictionary<string, object> GetParameterForDirection(ParameterDirection direction)
        {
            return this.exposedParameters.Where(
                p => p.Direction == direction).
                ToDictionary(
                    s => s.Name, 
                    s => this.Get<object>(s.Name));
        }
    }
}
