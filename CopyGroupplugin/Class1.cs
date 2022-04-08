using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyGroupplugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CopyGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;

                GroupPickFilter groupPickFilter = new GroupPickFilter();
                Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, groupPickFilter, "select group of objects");
                Element element = doc.GetElement(reference);
                Group group = element as Group;
                XYZ groupCenter = GetElemCenter(group);
                Room room = GetRoomByPoint(doc, groupCenter);
                XYZ roomCenter = GetElemCenter(room);
                XYZ offset = groupCenter - roomCenter;

                XYZ point = uiDoc.Selection.PickPoint("select point");
                Room selectedRoom = GetRoomByPoint(doc, point);
                XYZ selectedRoomCenter = GetElemCenter(selectedRoom); 
                XYZ insertPoint = selectedRoomCenter + offset; 


                Transaction ts = new Transaction(doc);
                ts.Start("group copy");
                doc.Create.PlaceGroup(insertPoint, group.GroupType);
                ts.Commit();

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;   
            }
            catch(Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        public class GroupPickFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
                    return true;
                else
                    return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                throw new NotImplementedException();
            }
        }

        public XYZ GetElemCenter(Element element)
        {
            BoundingBoxXYZ bounding =  element.get_BoundingBox(null);
            return (bounding.Max+bounding.Min)/2;
        }

        public Room GetRoomByPoint(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);
            foreach(Element e in collector)
            {
                Room room = e as Room;
                if(room != null)
                {
                    if (room.IsPointInRoom(point))
                        
                        return room;    
                    
                }
            }
            return null;    
        }
    }


}
