(function () {
    myscript = {

        getDistant: function (cpt, bl) {
            var vY = bl[1].lat - bl[0].lat,
				vX = bl[0].lng - bl[1].lng;
            return (vX * (cpt.lat - bl[0].lat) + vY * (cpt.lng - bl[0].lng));
        },

        findMostDistantPointFromBaseLine: function (baseLine, latLngs) {
            var maxD = 0,
				maxPt = null,
				newPoints = [],
				i, pt, d;

            for (i = latLngs.length - 1; i >= 0; i--) {
                pt = latLngs[i];
                d = this.getDistant(pt, baseLine);

                if (d > 0) {
                    newPoints.push(pt);
                } else {
                    continue;
                }

                if (d > maxD) {
                    maxD = d;
                    maxPt = pt;
                }
            }

            return { maxPoint: maxPt, newPoints: newPoints };
        },

        buildConvex: function (baseLine, latLngs) {
            var convexBaseLines = [],
				t = this.findMostDistantPointFromBaseLine(baseLine, latLngs);

            if (t.maxPoint) {
                convexBaseLines =
					convexBaseLines.concat(
						this.buildConvex([baseLine[0], t.maxPoint], t.newPoints)
					);
                convexBaseLines =
					convexBaseLines.concat(
						this.buildConvex([t.maxPoint, baseLine[1]], t.newPoints)
					);
                return convexBaseLines;
            } else {
                return [baseLine[0]];
            }
        },

        getBorder: function (latLngs) {
            var maxLat = false, minLat = false,
				maxPt = null, minPt = null,
				i;

            for (i = latLngs.length - 1; i >= 0; i--) {
                var pt = latLngs[i];
                if (maxLat === false || pt.lat > maxLat) {
                    maxPt = pt;
                    maxLat = pt.lat;
                }
                if (minLat === false || pt.lat < minLat) {
                    minPt = pt;
                    minLat = pt.lat;
                }
            }
            var ch = [].concat(this.buildConvex([minPt, maxPt], latLngs),
								this.buildConvex([maxPt, minPt], latLngs));

            return ch;
        },

        getColor: function (number) {
            return '#' + Math.floor(Math.random() * 16777215).toString(16);
        }
    };
}());
