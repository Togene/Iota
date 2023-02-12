using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Surface {
     private List<Face> Faces = new ();
        private int        SurfaceType;
        
        List<Face> frontList = new ();
        List<Face> backList  = new ();
        List<Face> leftList  = new ();
        List<Face> rightList = new ();
        
        public Surface(int type) {
            SurfaceType = type;
        }

        Vector3 TypeToStepVector(string s) {
            switch (s) {
                case "front":
                    switch (SurfaceType) {
                        case 0: case 1: // top and bottom
                            return Vector3.forward;
                        case 2: case 3: // front and back
                            return Vector3.up;
                        case 4: case 5: // left and right
                            return Vector3.up;
                    }
                    Debug.LogError("surface type not recognized");
                    return Vector3.zero;
                case "back":
                    switch (SurfaceType) {
                        case 0: case 1: // top and bottom
                            return Vector3.back;
                        case 2: case 3: // front and back
                            return Vector3.down;
                        case 4: case 5: // left and right
                            return Vector3.down;
                    }
                    Debug.LogError("surface type not recognized");
                    return Vector3.zero;
                case "left":
                    switch (SurfaceType) {
                        case 0: case 1: // top and bottom
                            return Vector3.left;
                        case 2: case 3: // front and back
                            return Vector3.left;
                        case 4: case 5: // left and right
                            return Vector3.forward;
                    }
                    Debug.LogError("surface type not recognized");
                    return Vector3.zero;
                case "right":
                    switch (SurfaceType) {
                        case 0: case 1: // top and bottom
                            return Vector3.right;
                        case 2: case 3: // front and back
                            return Vector3.right;
                        case 4: case 5: // left and right
                            return Vector3.back;
                    }
                    Debug.LogError("surface type not recognized");
                    return Vector3.zero;
                default:
                    Debug.LogError("step key not recognized");
                    return Vector3.zero;
            }
        }
        
        public void SurfaceHop(string pKey, ref Points points, bool skipStart = true) {
            Vector3 frontStep = TypeToStepVector("front");
            Vector3 backStep  = TypeToStepVector("back");
            Vector3 leftStep  = TypeToStepVector("left");
            Vector3 rigtStep  = TypeToStepVector("right");
            
            List<Face> localFront = new List<Face>();
            List<Face> localBack  = new List<Face>();
            List<Face> localLeft  = new List<Face>();
            List<Face> localRight = new List<Face>();
            
            points.Hop(ref localFront, points[pKey].Position, frontStep, SurfaceType, false);
            points.Hop(ref localBack, points[pKey].Position, backStep, SurfaceType, false);
            points.Hop(ref localLeft, points[pKey].Position, leftStep, SurfaceType, false);
            points.Hop(ref localRight, points[pKey].Position, rigtStep, SurfaceType, false);
            
            if (localFront.Count != 0) {
                List<Face> tangentLeft  = new List<Face>();
                List<Face> tangentRight = new List<Face>();
                foreach (var fp in localFront) {
                    points.Hop(ref tangentLeft, points[fp.PKey].Position, leftStep, SurfaceType, false);
                    points.Hop(ref tangentRight, points[fp.PKey].Position, rigtStep, SurfaceType, false);
                }
                
                if (tangentLeft.Count != 0) {
                    localLeft.AddRange((tangentLeft));
                }
                if (tangentRight.Count != 0) {
                    localRight.AddRange((tangentRight));
                }
                AddFront(EvalualatePoints(localFront));
            }
            
            if (localBack.Count != 0) {
                List<Face> tangentLeft  = new List<Face>();
                List<Face> tangentRight = new List<Face>();
                foreach (var fp in localBack) {
                    points.Hop(ref tangentLeft, points[fp.PKey].Position, leftStep, SurfaceType, false);
                    points.Hop(ref tangentRight, points[fp.PKey].Position, rigtStep, SurfaceType, false);
                }
                if (tangentLeft.Count != 0) {
                    localLeft.AddRange((tangentLeft));
                }
                if (tangentRight.Count != 0) {
                    localRight.AddRange((tangentRight));
                }
                AddBack(EvalualatePoints(localBack));
            }
            
            if (localLeft.Count != 0) {
                AddLeft(EvalualatePoints(localLeft));
            }
            if (localRight.Count != 0) {
                AddRight(EvalualatePoints(localRight));
            }
        }
        
        public void AddFront(List<Face> p) {
            frontList.AddRange(p);
        }
        public void AddBack(List<Face> p) {
            backList.AddRange(p);
        }
        public void AddLeft(List<Face> p) {
            leftList.AddRange(p);
        }
        public void AddRight(List<Face> p) {
            rightList.AddRange(p);
        }
        
        bool MergePoints(ref Face vp, ref Face vp1) {
            var TL1 = vp.indices[0];
            var TR1 = vp.indices[1];
            var BR1 = vp.indices[2];
            var BL1 = vp.indices[3];
                     
            var TL2 = vp1.indices[0];
            var TR2 = vp1.indices[1];
            var BR2 = vp1.indices[2];
            var BL2 = vp1.indices[3];
            
            bool connected = false;

            // vp1 on bottom        
            if (BL1 == TL2 && BR1 == TR2) {
                vp.indices[2] = BR2;        
                vp.indices[3] = BL2;
                connected                        = true;
            }
            // vp1 on top
            if (TL1 == BL2 && TR1 == BR2) {
                vp.indices[0] = TL2;        
                vp.indices[1] = TR2;
                connected                        = true;
            }
            // vp1 on right
            if (TR1 == TL2 && BR1 == BL2) {
                vp.indices[1] = TR2;        
                vp.indices[2] = BR2;
                connected                        = true;
            }
            // vp1 on left 
            if (TL1 == TR2 && BL1 == BR2) {
                vp.indices[0] = TL2;        
                vp.indices[3] = BL2;
                connected                        = true;
            }
            
            // vp.CalculateSize();
            // vp1.CalculateSize();
            return connected;
        }
        
        List<Face> EvalualatePoints(List<Face> list) {
            if (list.Count == 1) {
                return list;
            }
            for (int x = 0; x < 2; x++) {
                for (int i = 0; i < list.ToList().Count; i++) {
                    var vp = list[i];
                    for (int k = 0; k < list.ToList().Count; k++) {
                        var vp1 = list[k];
                        if (i == k) {continue;}
                        if (MergePoints(ref vp, ref vp1)) {
                            list.Remove(vp1);
                        }
                    }
                }
            }
            return list;
        }

        public bool Buffered() {
            return frontList.Count != 0 || backList.Count != 0 || leftList.Count != 0 || rightList.Count != 0;
        }

        public List<Face> GetSurfaceFaces() {
            var faces = new List<Face>();
            
            if (frontList.Count != 0) {
                Faces.AddRange(EvalualatePoints(frontList));
            }
            if (backList.Count != 0) {
                Faces.AddRange(EvalualatePoints(backList));
            }
            if (leftList.Count != 0) {
                Faces.AddRange(EvalualatePoints(leftList));
            }
            if (rightList.Count != 0) {
                Faces.AddRange(EvalualatePoints(rightList));
            }
            
            var final = new List<Face>();
            final.AddRange(EvalualatePoints(Faces));
            foreach (var face in final) {
                faces.Add(face);
            }
            return faces;
        }
}
