using System.Linq;
using System.Collections.Generic;
using UnityEngine;


//     0 + step, -- Top Left        // 0
//     1 + step, -- Top Right       // 1
//     2 + step, -- Bottom Right    // 2
//     0 + step, -- Top Left        // 3
//     2 + step, -- Bottom Right    // 4
//     3 + step  -- Bottom Left     // 5
public class Points {
 public  Dictionary<string, Vertex> VerticesKeyMap = new();
    public  Vertex[]                   Vertices         = new Vertex[]{};
    private Dictionary<string, Point>  PointsList       = new();
  
    
    public ref Dictionary<string, Point> GetList() {
        return ref PointsList;
    }

    // public int ListCount() {
    //     return VerticesKeyMap.Count;
    // }
    //
    // public bool EmpyOrNull() {
    //     return VerticesKeyMap == null || VerticesKeyMap.Count != 0;
    // }
    //
    // public  List<Point> GetPointList() {
    //     return  PointsList.Values.ToList();
    // }

    public Point this[string a] {
        get { return PointsList[a]; }
        set { PointsList[a] = value; }
    }

    public void Add(Vector3 v) {
        PointsList.Add(v.Key(), new Point(v, true));
    }

    public void ClearCheck() {
        foreach (var p in PointsList) {
            p.Value.FacesChecked[0] = false;
            p.Value.FacesChecked[1] = false;
            p.Value.FacesChecked[2] = false;
            p.Value.FacesChecked[3] = false;
            p.Value.FacesChecked[4] = false;
            p.Value.FacesChecked[5] = false;
        }
    }
    // 😭 
    bool VerticeCondition(bool a, bool b, bool c) {
        return a && b && c || !a && b && c || !a && !b && c || !a && b && !c || a && !b && !c;
    }
    
    int AddVertice(Vertex v, bool real, int type) {
        if (!VerticesKeyMap.ContainsKey(v.Key)) {
            
            v.Virtual = !real;
            v.Type    = type;
            
            VerticesKeyMap.Add(v.Key, v);
            Vertices = VerticesKeyMap.Values.ToArray();
            v.Index  = VerticesKeyMap.Count - 1;

            return VerticesKeyMap.Count - 1;
        }
        return VerticesKeyMap[v.Key].Index;
    }
    
    Dictionary<int, int> CellState(Point p) {
        Dictionary<int, int> caseIndex = new();
            // TL
            caseIndex.Add(0, AddVertice(p.Position + (Vector3.forward + Vector3.left + Vector3.up) / 2, 
                VerticeCondition(!ForwardLeft(p), !Left(p), !Forward(p)), 0));
            // TR
            caseIndex.Add(1, AddVertice(p.Position + (Vector3.forward + Vector3.right + Vector3.up) / 2,
                VerticeCondition(!ForwardRight(p), !Right(p), !Forward(p)), 1));
            // BR
            caseIndex.Add(2, AddVertice(p.Position + (Vector3.back + Vector3.right + Vector3.up) / 2, 
                VerticeCondition(!BackRight(p), !Right(p), !Back(p)), 2));
            // BL
            caseIndex.Add(3,AddVertice(p.Position + (Vector3.back + Vector3.left + Vector3.up) / 2,
                VerticeCondition(!BackLeft(p), !Left(p), !Back(p)), 3));
            // TL
            caseIndex.Add(4, AddVertice(p.Position + (Vector3.forward + Vector3.left + Vector3.down) / 2, 
                VerticeCondition(!ForwardLeft(p), !Left(p), !Forward(p)), 4));
            // TR
            caseIndex.Add(5, AddVertice(p.Position + (Vector3.forward + Vector3.right + Vector3.down) / 2,
                VerticeCondition(!ForwardRight(p), !Right(p), !Forward(p)), 5));
            // BR
            caseIndex.Add(6, AddVertice(p.Position + (Vector3.back + Vector3.right + Vector3.down) / 2, 
                VerticeCondition(!BackRight(p), !Right(p), !Back(p)), 6));
            // BL
            caseIndex.Add(7,AddVertice(p.Position + (Vector3.back + Vector3.left + Vector3.down) / 2,
                VerticeCondition(!BackLeft(p), !Left(p), !Back(p)), 7));
            return caseIndex; 
    }

    
    public bool ActiveByCheckType(int i, Vector3 v) {
        switch (i) {
            case 0: // top
                return Top(PointsList[v.Key()]);
            case 1: // bottom
                return Down(PointsList[v.Key()]);
            case 2: // front
                return Forward(PointsList[v.Key()]);
            case 3: // front
                return Back(PointsList[v.Key()]);
            case 4: // left
                return Left(PointsList[v.Key()]);
            case 5: // back
                return Right(PointsList[v.Key()]);
            default:
                return false;
        }
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
            return connected;
        }
        
    void EvalualatePoints(Face vp, ref List<Face> list) {
        for (int k = 0; k < list.Count; k++) {
            var vp1 = list[k];
            if (MergePoints(ref vp1, ref vp)) {
                return;
            }
        }
        list.Add(vp);
    }
        
    public void Hop(ref List<Face> ps, Vector3 start, Vector3 step, int checkType, bool skipStart = true) {
        
        Vector3 next = start;
        if (skipStart) {
             next = start + step;
        }
        
        if (ContainsAndActive(next) && !Checked(next, checkType) && !ActiveByCheckType(checkType, next)) {
            var p       = PointsList[(next).Key()];
            var indices = CellState(p);
            switch (checkType) {
                case 0: // top face
                    p.SetFaces(checkType,
                        new [] {
                            Vertices[indices[0]], Vertices[indices[1]], Vertices[indices[2]], Vertices[indices[3]],
                        });
                    break;
                case 1: // bottom face
                    p.SetFaces(checkType,
                        new [] {
                            Vertices[indices[7]], Vertices[indices[6]], Vertices[indices[5]], Vertices[indices[4]],
                        });
                    
                    break;
                case 2: // front face
                    p.SetFaces(checkType,
                        new [] {
                            Vertices[indices[1]], Vertices[indices[0]], Vertices[indices[4]], Vertices[indices[5]],
                        });
                    break;
                case 3: // back face
                    p.SetFaces(checkType,
                        new [] {
                            Vertices[indices[3]], Vertices[indices[2]], Vertices[indices[6]], Vertices[indices[7]],
                        });
                    break;
                case 4: // left face
                    p.SetFaces(checkType,
                        new [] {
                            Vertices[indices[0]], Vertices[indices[3]], Vertices[indices[7]], Vertices[indices[4]],
                        });
                    break;
                case 5: // right face
                    p.SetFaces(checkType,
                        new [] {
                            Vertices[indices[2]], Vertices[indices[1]], Vertices[indices[5]], Vertices[indices[6]],
                        });
                    break;
            }
            EvalualatePoints(p.Faces[checkType], ref ps);
            Hop(ref ps, p.Position, step, checkType);
            PointsList[(next).Key()].FacesChecked[checkType] = true;
        }
    }

    public Point GetPointByVector(Vector3 v) {
        return PointsList[v.Key()];
    }
    public bool Contains(Vector3 v) {
        return PointsList.ContainsKey(v.Key());
    }
    public bool IsActive(Vector3 v) {
        return PointsList[v.Key()].Active;
    }
    public bool Checked(Vector3 v, int checkType) {
        return PointsList[v.Key()].FacesChecked[checkType];
    }
    public void SetPointsActive(Vector3 v, bool b) {
        PointsList[v.Key()].Active = b;
    }
    public bool ContainsAndActive(Vector3 v) {
        return Contains(v) && IsActive(v);
    }
    public bool Top(Point p) {
         return ContainsAndActive(p.Position + Vector3.up); 
    }
    public bool Down(Point p) {
        return ContainsAndActive(p.Position + Vector3.down);
    }
    public bool Right(Point p) {
        return ContainsAndActive(p.Position + Vector3.right);
    }
    public bool Left(Point p) {
        return ContainsAndActive(p.Position + Vector3.left);
    }
    public bool Forward(Point p) {
        return ContainsAndActive(p.Position + Vector3.forward);
    }
    public bool Back(Point p) {
        return ContainsAndActive(p.Position + Vector3.back);
    }
    bool ForwardRight(Point p) {
        return ContainsAndActive(p.Position + Vector3.forward + Vector3.right);
    }
    bool ForwardLeft(Point p) {
        return ContainsAndActive(p.Position + Vector3.forward + Vector3.left);
    }
    bool BackRight(Point p) {
        return ContainsAndActive(p.Position + Vector3.back + Vector3.right);
    }
    bool BackLeft(Point p) {
        return ContainsAndActive(p.Position + Vector3.back + Vector3.left);
    }
}
