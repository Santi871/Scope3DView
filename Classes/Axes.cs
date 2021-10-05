using System;
using NINA.Core.Enum;
using NINA.Equipment.Interfaces.Mediator;

namespace Scope3DView.Classes
{
    public class Axes
    {
        private readonly ITelescopeMediator _telescopeMediator;
        private bool SouthernHemisphere => _telescopeMediator.GetInfo().SiteLatitude < 0;

        public Axes(ITelescopeMediator telescopeMediator)
        {
            _telescopeMediator = telescopeMediator;
        }

        /// <summary>
        /// convert a RaDec position to an axes positions. 
        /// </summary>
        /// <param name="raDec"></param>
        /// <returns></returns>
        internal double[] RaDecToAxesXY(AlignMode mode, double[] raDec)
        {
            var axes = new[] {raDec[0], raDec[1]};
            switch (mode)
            {
                case AlignMode.algGermanPolar:
                    axes[0] = (_telescopeMediator.GetInfo().SiderealTime - axes[0]) * 15.0;
                    if (SouthernHemisphere)
                    {
                        axes[1] = -axes[1];
                    }
                    
                    var axes3 = GetAltAxisPosition(axes);

                    // small hack to prevent the 3D model's RA from flipping upside down when parked
                    // seems to be an issue with the way ASCOM reports the pier side at dec == 90
                    // requires further investigation
                    var pierSide = _telescopeMediator.GetInfo().SideOfPier;
                    if (Math.Abs(raDec[1] - (-90)) < 0.00001 && pierSide == PierSide.pierEast)
                        pierSide = PierSide.pierWest;

                    switch (pierSide)
                    {
                        case PierSide.pierUnknown:
                            break;
                        case PierSide.pierEast:
                            if (SouthernHemisphere)
                            {
                                // southern
                                axes[0] = axes[0];
                                axes[1] = axes[1];
                            }
                            else
                            {
                                axes[0] = axes[0];
                                axes[1] = axes[1];
                            }

                            break;
                        case PierSide.pierWest:
                            if (SouthernHemisphere)
                            {
                                axes[0] = axes3[0];
                                axes[1] = axes3[1];
                            }
                            else
                            {
                                axes[0] = axes3[0];
                                axes[1] = axes3[1];
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return axes;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// GEMs have two possible axes positions, given an axis position this returns the other 
        /// </summary>
        /// <param name="alt">position</param>
        /// <returns>other axis position</returns>
        private static double[] GetAltAxisPosition(double[] alt)
        {
            var d = new[] {0.0, 0.0};
            if (alt[0] > 90)
            {
                d[0] = alt[0] - 180;
                d[1] = 180 - alt[1];
            }
            else
            {
                d[0] = alt[0] + 180;
                d[1] = 180 - alt[1];
            }

            return d;
        }
    }
    
    public enum AlignMode
    {
        algGermanPolar
    }
}