﻿//******************************************************************************************************
//  PointTagConverter.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/11/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Globalization;
using System.Windows.Data;

namespace GSF.TimeSeries.UI.Converters
{
    public class PointTagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string pointTag = value as string;

            if ((object)pointTag != null)
            {
                int colon = pointTag.LastIndexOf(':');
                int start = pointTag.IndexOf('!');
                int stop;

                if (colon >= 0)
                {
                    stop = pointTag.Substring(0, colon).LastIndexOf('!');

                    if (start >= 0 && start < stop)
                        return string.Format("{0}!...{1}", pointTag.Substring(0, start), pointTag.Substring(stop));
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
