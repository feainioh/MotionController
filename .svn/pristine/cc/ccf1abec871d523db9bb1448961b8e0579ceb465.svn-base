using System;
using System.Collections.Generic;
using System.Text;
using DWGdirect.DatabaseServices;
using Microsoft.Win32;
using DWGdirect.GraphicsInterface;
using DWGdirect.Runtime;
using DWGdirect.GraphicsSystem;
using DWGdirect.Geometry;
using DWGdirect.Colors;
using System.IO;

namespace OQC_IC_CHECK_System
{
    class DwgFuncApplication
    {
    }

    class HostAppServ : HostApplicationServices
    {
        DWGdirect.Runtime.Services dd;
        public HostAppServ(DWGdirect.Runtime.Services serv)
        {
            dd = serv;
        }

        public String FindConfigPath(String configType)
        {
            String subkey = GetRegistryAcadProfilesKey();
            if (subkey.Length > 0)
            {
                subkey += String.Format("\\General");
                String searchPath;
                if (GetRegistryString(Registry.CurrentUser, subkey, configType, out searchPath))
                    return searchPath;
            }
            return String.Format("");
        }

        private String FindConfigFile(String configType, String file)
        {
            String searchPath = FindConfigPath(configType);
            if (searchPath.Length > 0)
            {
                searchPath = String.Format("{0}\\{1}", searchPath, file);
                if (dd.AccessFileRead(searchPath))
                    return searchPath;
            }
            return String.Format("");
        }

        public override String FindFile(String file, Database db, FindFileHint hint)
        {
            String sFile = this.sFindFile;
            if (sFile.Length > 0)
                return sFile;

            String strFileName = file;
            String ext;
            if (strFileName.Length > 3)
                ext = strFileName.Substring(strFileName.Length - 4, 4).ToUpper();
            else
                ext = file.ToUpper();
            if (ext == String.Format(".PC3"))
                return FindConfigFile(String.Format("PrinterConfigDir"), file);
            if (ext == String.Format(".STB") || ext == String.Format(".CTB"))
                return FindConfigFile(String.Format("PrinterStyleSheetDir"), file);
            if (ext == String.Format(".PMP"))
                return FindConfigFile(String.Format("PrinterDescDir"), file);

            switch (hint)
            {
                case FindFileHint.FontFile:
                case FindFileHint.CompiledShapeFile:
                case FindFileHint.TrueTypeFontFile:
                case FindFileHint.PatternFile:
                case FindFileHint.FontMapFile:
                case FindFileHint.TextureMapFile:
                    break;
                default:
                    return sFile;
            }

            if (hint != FindFileHint.TextureMapFile && ext != String.Format(".SHX") && ext != String.Format(".PAT") && ext != String.Format(".TTF") && ext != String.Format(".TTC"))
            {
                strFileName += String.Format(".shx");
            }
            else if (hint == FindFileHint.TextureMapFile)
            {
                strFileName.Replace(String.Format("/"), String.Format("\\"));
                int last = strFileName.LastIndexOf("\\");
                strFileName = strFileName.Substring(0, last);
            }


            sFile = (hint != FindFileHint.TextureMapFile) ? GetRegistryACADFromProfile() : GetRegistryAVEMAPSFromProfile();
            while (sFile.Length > 0)
            {
                int nFindStr = sFile.IndexOf(";");
                String sPath;
                if (-1 == nFindStr)
                {
                    sPath = sFile;
                    sFile = String.Format("");
                }
                else
                {
                    sPath = String.Format("{0}\\{1}", sFile.Substring(0, nFindStr), strFileName);
                    if (dd.AccessFileRead(sPath))
                    {
                        return sPath;
                    }
                    sFile = sFile.Substring(nFindStr + 1, sFile.Length - nFindStr - 1);
                }
            }

            if (hint == FindFileHint.TextureMapFile)
            {
                return sFile;
            }

            if (sFile.Length <= 0)
            {
                String sAcadLocation = GetRegistryAcadLocation();
                if (sAcadLocation.Length > 0)
                {
                    sFile = String.Format("{0}\\Fonts\\{1}", sAcadLocation, strFileName);
                    if (dd.AccessFileRead(sFile))
                    {
                        sFile = String.Format("{0}\\Support\\{1}", sAcadLocation, strFileName);
                        if (dd.AccessFileRead(sFile))
                        {
                            sFile = String.Format("");
                        }
                    }
                }
            }
            return sFile;
        }

        public override String GetPassword(String dwgName, PasswordOptions options)
        {
            //PasswordDlg pwdDlg = new PasswordDlg();
            //pwdDlg.TextFileName.Text = dwgName;
            //if (pwdDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    return pwdDlg.password.Text;
            //}
            return String.Format("");
        }

        public override String FontMapFileName
        {
            get
            {
                String subkey = GetRegistryAcadProfilesKey();
                if (subkey.Length > 0)
                {
                    subkey += String.Format("\\Editor Configuration");
                    String fontMapFile;
                    if (GetRegistryString(Registry.CurrentUser, subkey, String.Format("FontMappingFile"), out fontMapFile))
                        return fontMapFile;
                }
                return String.Format("");
            }
        }

        bool GetRegistryString(RegistryKey rKey, String subkey, String name, out String value)
        {
            bool rv = false;
            object objData = null;

            RegistryKey regKey;
            regKey = rKey.OpenSubKey(subkey);
            if (regKey != null)
            {
                objData = regKey.GetValue(name);
                if (objData != null)
                {
                    rv = true;
                }
                regKey.Close();
            }
            if (rv)
                value = objData.ToString();
            else
                value = String.Format("");

            rKey.Close();
            return rv;
        }

        String GetRegistryAVEMAPSFromProfile()
        {
            String subkey = GetRegistryAcadProfilesKey();
            if (subkey.Length > 0)
            {
                subkey += String.Format("\\General");
                // get the value for the ACAD entry in the registry
                String tmp;
                if (GetRegistryString(Registry.CurrentUser, subkey, String.Format("AVEMAPS"), out tmp))
                    return tmp;
            }
            return String.Format("");
        }

        String GetRegistryAcadProfilesKey()
        {
            String subkey = String.Format("SOFTWARE\\Autodesk\\AutoCAD");
            String tmp;

            if (!GetRegistryString(Registry.CurrentUser, subkey, String.Format("CurVer"), out tmp))
                return String.Format("");
            subkey += String.Format("\\{0}", tmp);

            if (!GetRegistryString(Registry.CurrentUser, subkey, String.Format("CurVer"), out tmp))
                return String.Format("");
            subkey += String.Format("\\{0}\\Profiles", tmp);

            if (!GetRegistryString(Registry.CurrentUser, subkey, String.Format(""), out tmp))
                return String.Format("");
            subkey += String.Format("\\{0}", tmp);
            return subkey;
        }

        String GetRegistryAcadLocation()
        {
            String subkey = String.Format("SOFTWARE\\Autodesk\\AutoCAD");
            String tmp;

            if (!GetRegistryString(Registry.CurrentUser, subkey, String.Format("CurVer"), out tmp))
                return String.Format("");
            subkey += String.Format("\\{0}", tmp);

            if (!GetRegistryString(Registry.CurrentUser, subkey, String.Format("CurVer"), out tmp))
                return String.Format("");
            subkey += String.Format("\\{0}", tmp);

            if (!GetRegistryString(Registry.CurrentUser, subkey, String.Format(""), out tmp))
                return String.Format("");
            return tmp;
        }

        String GetRegistryACADFromProfile()
        {
            String subkey = GetRegistryAcadProfilesKey();
            if (subkey.Length > 0)
            {
                subkey += String.Format("\\General");
                // get the value for the ACAD entry in the registry
                String tmp;
                if (GetRegistryString(Registry.CurrentUser, subkey, String.Format("ACAD"), out tmp))
                    return tmp;
            }
            return String.Format("");
        }
    }

    class Aux
    {
        public static ObjectId active_viewport_id(Database database)
        {
            if (database.TileMode)
            {
                return database.CurrentViewportTableRecordId;
            }
            else
            {
                using (BlockTableRecord paperBTR = (BlockTableRecord)database.CurrentSpaceId.GetObject(OpenMode.ForWrite))
                {
                    Layout l = (Layout)paperBTR.LayoutId.GetObject(OpenMode.ForWrite);
                    return l.CurrentViewportId;
                }
            }
        }

        public static void preparePlotstyles(Database database, ContextForDbDatabase ctx)
        {
            try
            {
                using (BlockTableRecord paperBTR = (BlockTableRecord)database.CurrentSpaceId.GetObject(OpenMode.ForRead))
                {
                    using (Layout pLayout = (Layout)paperBTR.LayoutId.GetObject(OpenMode.ForRead))
                    {
                        if (ctx.IsPlotGeneration ? pLayout.PlotPlotStyles : pLayout.ShowPlotStyles)
                        {
                            String pssFile = pLayout.CurrentStyleSheet;
                            if (pssFile.Length > 0)
                            {
                                String testpath = ((HostAppServ)HostApplicationServices.Current).FindFile(pssFile, database, FindFileHint.Default);
                                if (testpath.Length > 0)
                                {
                                    using (FileStreamBuf pFileBuf = new FileStreamBuf(testpath))
                                    {
                                        ctx.LoadPlotStyleTable(pFileBuf);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }

    class SR : SelectionReactor
    {
        ObjectIdCollection selected;
        ObjectId spaceId;
        public SR(ObjectIdCollection s, ObjectId id)
        {
            spaceId = id;
            selected = s;
        }
        public override bool Selected(DrawableDesc pDrawableDesc)
        {
            DrawableDesc pDesc = pDrawableDesc;
            if (pDesc.Parent != null)
            {
                // we walk up the GS node path to the root container primitive
                // to avoid e.g. selection of individual lines in a dimension 
                while (((DrawableDesc)pDesc.Parent).Parent != null)
                    pDesc = (DrawableDesc)pDesc.Parent;
                if (pDesc.PersistId != IntPtr.Zero && ((DrawableDesc)pDesc.Parent).PersistId == spaceId.OldIdPtr)
                {
                    pDesc.MarkedToSkip = true; // regen abort for selected drawable, to avoid duplicates
                    selected.Add(new ObjectId(pDesc.PersistId));
                }
                return true;
            }
            return false;
        }
        // this more informative callback may be used to implement subentities selection
        public override SelectionReactorResult Selected(PathNode __p1, DWGdirect.GraphicsInterface.Viewport __p2)
        {
            return SelectionReactorResult.NotImplemented;
        }
        //public void drawLine(Database pDb)
        //{
        //    ObjectId odaLayer1Id = addLayer(pDb, "Oda Layer 1", 1/*TODO*/, "CONTINUOUS");
        //    ObjectId ttfStyleId = addStyle(pDb, "OdaTtfStyle", 0.0, 1.0, 0.2, 0.0,
        //        "VERDANA.TTF", false, "Verdana", false, false, 0, 34);
        //    using (BlockTable blockTable = (BlockTable)pDb.BlockTableId.GetObject(OpenMode.ForRead))
        //    {
        //        ObjectId modelSpaceId = blockTable[BlockTableRecord.ModelSpace];

        //        createEntityBoxes(modelSpaceId, odaLayer1Id);

        //        /**********************************************************************/
        //        /* Add some lines                                                     */
        //        /**********************************************************************/
        //        addLines(modelSpaceId, 0, 0, odaLayer1Id, ttfStyleId);
        //    }
        //}
    };

    public partial class DbFiller
    {
        EntityBoxes m_EntityBoxes;
        double m_textSize;
        Vector3d m_textOffset;
        Vector3d m_textLine;
        ObjectIdCollection m_layoutEntities;

        public DbFiller()
        {
            m_EntityBoxes = new EntityBoxes();
            m_textSize = 0.2;
            m_textOffset = new Vector3d(0.5 * m_textSize, -0.5 * m_textSize, 0);
            m_textLine = new Vector3d(0, -1.6 * m_textSize, 0);
            m_layoutEntities = new ObjectIdCollection();
        }

        static double OdaToRadian(double dAngle)
        {
            return dAngle * Math.PI / 180;
        }

        /// <summary>
        /// 点间画线
        /// </summary>
        /// <param name="pDb"></param>
        /// <param name="point_start"></param>
        /// <param name="point_end"></param>
        public Line CreateLine(ref Database pDb, Point3d point_start, Point3d point_end, string layerName)
        {
            LayerTable pLayers = (LayerTable)pDb.LayerTableId.GetObject(OpenMode.ForRead);
            ObjectId odaLayer1Id;
            if (pLayers[layerName].Handle.Value == 0)
            {
                //如果不存在对应图层，增加一个图层用来画图
                odaLayer1Id = addLayer(pDb, layerName, 1/*TODO*/, "CONTINUOUS");
                //设置画图的参数，包括尺寸，对象名称等--stylename不能重复
                ObjectId ttfStyleId = addStyle(pDb, "OdaTtfStyle_" + layerName.ToString(), 0.0, 1.0, 0.2, 0.0,
                    "VERDANA.TTF", false, "Verdana", false, false, 0, 34);
            }
            else
            {
                odaLayer1Id = pLayers[layerName].GetObject(OpenMode.ForWrite).Id;
            }

            using (BlockTable blockTable = (BlockTable)pDb.BlockTableId.GetObject(OpenMode.ForRead))
            {
                ObjectId modelSpaceId = blockTable[BlockTableRecord.ModelSpace];
                //createEntityBoxes(modelSpaceId, odaLayer1Id);
                //addLine(modelSpaceId, odaLayer1Id, ttfStyleId, point_start, point_end);

                //在当前的图层中画图
                Line pLine = new Line(point_start, point_end);
                pLine.Color = Color.FromColor(System.Drawing.Color.DarkRed);
                using (Transaction transaction =
                    HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                {
                    BlockTableRecord blockTableRecord;
                    //打开current BlockTableRecord
                    blockTableRecord = (BlockTableRecord)transaction.GetObject(
                        HostApplicationServices.WorkingDatabase.CurrentSpaceId,
                        OpenMode.ForWrite);

                    blockTableRecord.AppendEntity(pLine);
                    //設置實體所在圖層
                    pLine.LayerId = odaLayer1Id;
                    transaction.AddNewlyCreatedDBObject(pLine, true);
                    transaction.Commit();
                }
                return pLine;
            }
        }

        /***********************************************************************/
        /* 在指定的BlockTableRecord中添加圆                                     */
        /***********************************************************************/
        public Circle CreateCircle(ref Database pDb, Point3d point_center, double radio, string layerName)
        {
            //增加一个图层用来画图
            ObjectId odaLayer1Id = addLayer(pDb, layerName, 1/*TODO*/, "CONTINUOUS");
            //设置画图的参数，包括尺寸，对象名称等--stylename不能重复
            ObjectId ttfStyleId = addStyle(pDb, "OdaTtfStyle_" + layerName.ToString(), 0.0, 1.0, 0.2, 0.0,
                "VERDANA.TTF", false, "Verdana", false, false, 0, 34);
            using (BlockTable blockTable = (BlockTable)pDb.BlockTableId.GetObject(OpenMode.ForRead))
            {
                ObjectId modelSpaceId = blockTable[BlockTableRecord.ModelSpace];
                Circle pCircle = new Circle();
                pCircle.SetDatabaseDefaults(pDb);
                pCircle.Radius = radio;
                pCircle.Center = point_center;
                pCircle.Color = Color.FromColor(System.Drawing.Color.Red);

                using (Transaction transaction =
                    HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                {
                    BlockTableRecord blockTableRecord;
                    //打开current BlockTableRecord
                    blockTableRecord = (BlockTableRecord)transaction.GetObject(
                        HostApplicationServices.WorkingDatabase.CurrentSpaceId,
                        OpenMode.ForWrite);

                    /*实体Entity--例如圆、直线、矩形等，它们都具有一些共同的属性，包括颜色、线型、线型
                    比例、可见性、图层等，当添加实体到块表记录时，如果没有明确指定这些属性，AutoCAD会自动调用
                    SetDatabaseDefaults()函数来设置默认值 */
                    blockTableRecord.AppendEntity(pCircle);
                    transaction.AddNewlyCreatedDBObject(pCircle, true);
                    transaction.Commit();
                }
                return pCircle;
            }
        }

        /************************************************************************/
        /* Populate the Database                                                */
        /*                                                                      */
        /* PaperSpace Viewports                                                 */
        /* Linetypes with embedded shapes, and custom linetypes                 */
        /*                                                                      */
        /************************************************************************/
        public void fillDatabase(Database pDb)
        {
            Console.WriteLine("\n\nPopulating the database...");

            ///**********************************************************************/
            ///* Set Creation and Last Update times                                 */
            ///**********************************************************************/
            //OdDbDate date;

            //date.setDate(1, 1, 2006);
            //date.setTime(12, 0, 0, 0); 
            //date.localToUniversal();
            //odDbSetTDUCREATE(*pDb, date);

            //date.getUniversalTime();
            //odDbSetTDUUPDATE(*pDb, date);

            /**********************************************************************/
            /* Add some Registered Applications                                   */
            /**********************************************************************/
            addRegApp(pDb, "ODA");
            addRegApp(pDb, "AVE_FINISH"); // for materials

            /**********************************************************************/
            /* Add an SHX text style                                              */
            /**********************************************************************/
            ObjectId shxTextStyleId = addStyle(pDb, "OdaShxStyle", 0.0, 1.0, 0.2, 0.0, "txt");

            /**********************************************************************/
            /* Add a TTF text style                                               */
            /**********************************************************************/
            ObjectId ttfStyleId = addStyle(pDb, "OdaTtfStyle", 0.0, 1.0, 0.2, 0.0,
                "VERDANA.TTF", false, "Verdana", false, false, 0, 34);

            /**********************************************************************/
            /* Add a Shape file style for complex linetypes                       */
            /**********************************************************************/
            ObjectId shapeStyleId = addStyle(pDb, "", 0.0, 1.0, 0.2, 0.0, "ltypeshp.shx", true);

            /**********************************************************************/
            /* Add some linetypes                                                 */
            /**********************************************************************/
            addLinetypes(pDb, shapeStyleId, shxTextStyleId);

            /**********************************************************************/
            /* Add a Layer                                                        */
            /**********************************************************************/
            ObjectId odaLayer1Id = addLayer(pDb, "Oda Layer 1", 1/*TODO*/, "CONTINUOUS");

            /**********************************************************************/
            /* Add a block definition                                             */
            /**********************************************************************/
            ObjectId odaBlock1Id = addBlockDef(pDb, "OdaBlock1", 1, 2);

            /**********************************************************************/
            /* Add a DimensionStyle                                               */
            /**********************************************************************/
            ObjectId odaDimStyleId = addDimStyle(pDb, "OdaDimStyle");

            /**********************************************************************/
            /* Add an MLine style                                                 */
            /**********************************************************************/
            ObjectId odaMLineStyleId = addMLineStyle(pDb, "OdaStandard", "ODA Standard Style");

            /**********************************************************************/
            /* Add a Material                                                     */
            /**********************************************************************/
            ObjectId odaMaterialId = addMaterial(pDb, "OdaMateria", "ODA Defined Materia");

            /**********************************************************************/
            /* Add a PaperSpace Viewport                                          */
            /**********************************************************************/
            addPsViewport(pDb, odaLayer1Id);

            /**********************************************************************/
            /* Add ModelSpace Entity Boxes                                        */
            /**********************************************************************/
            using (BlockTable blockTable = (BlockTable)pDb.BlockTableId.GetObject(OpenMode.ForRead))
            {
                ObjectId modelSpaceId = blockTable[BlockTableRecord.ModelSpace];

                createEntityBoxes(modelSpaceId, odaLayer1Id);

                /**********************************************************************/
                /* Add some lines                                                     */
                /**********************************************************************/
                addLines(modelSpaceId, 0, 0, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a 2D (heavy) polyline                                          */
                /**********************************************************************/
                add2dPolyline(modelSpaceId, 0, 1, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a PolyFace Mesh                                                */
                /**********************************************************************/
                addPolyFaceMesh(modelSpaceId, 0, 2, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a PolygonMesh                                                  */
                /**********************************************************************/
                addPolygonMesh(modelSpaceId, 0, 3, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add some curves                                                    */
                /**********************************************************************/
                addCurves(modelSpaceId, 0, 4, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Tolerance                                                    */
                /**********************************************************************/
                addTolerance(modelSpaceId, 0, 5, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add some Leaders                                                   */
                /**********************************************************************/
                addLeaders(modelSpaceId, 0, 6, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add an Aligned Dimension                                           */
                /**********************************************************************/
                addAlignedDimension(modelSpaceId, 0, 7, odaLayer1Id, ttfStyleId, odaDimStyleId);

                /**********************************************************************/
                /* Add a MultiLine                                                    */
                /**********************************************************************/
                addMLine(modelSpaceId, 0, 8, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add an Arc Dimension                                               */
                /**********************************************************************/
                addArcDimension(modelSpaceId, 0, 9, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a 3D Polyline                                                  */
                /**********************************************************************/
                add3dPolyline(modelSpaceId, 1, 0, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add MText                                                          */
                /**********************************************************************/
                addMText(modelSpaceId, 1, 1, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Block Reference                                                */
                /**********************************************************************/
                addBlockRef(modelSpaceId, 1, 2, odaLayer1Id, ttfStyleId, odaBlock1Id);

                /**********************************************************************/
                /* Add Radial Dimension                                               */
                /**********************************************************************/
                addRadialDimension(modelSpaceId, 1, 3, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add 3D Face                                                        */
                /**********************************************************************/
                add3dFace(modelSpaceId, 1, 4, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Hatches                                                        */
                /**********************************************************************/
                addHatches(modelSpaceId, 2, 0, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add some text entities to ModelSpace                               */
                /**********************************************************************/
                addTextEnts(modelSpaceId, 2, 1, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Solid                                                          */
                /**********************************************************************/
                addSolid(modelSpaceId, 2, 3, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add an Rotated Dimension                                           */
                /**********************************************************************/
                addRotatedDimension(modelSpaceId, 2, 4, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add an Ray                                                         */
                /**********************************************************************/
                addRay(modelSpaceId, 3, 0, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a 3 Point Angular Dimension                                    */
                /**********************************************************************/
                add3PointAngularDimension(modelSpaceId, 3, 1, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Ordinate Dimensions                                            */
                /**********************************************************************/
                addOrdinateDimensions(modelSpaceId, 3, 2, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a Spline                                                       */
                /**********************************************************************/
                addSpline(modelSpaceId, 3, 3, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add some Traces                                                    */
                /**********************************************************************/
                addTraces(modelSpaceId, 3, 4, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a Polyline                                                     */
                /**********************************************************************/
                addPolyline(modelSpaceId, 3, 5, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a Wipeout                                                      */
                /**********************************************************************/
                addWipeout(modelSpaceId, 3, 7, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a RadialDimensionLarge                                         */
                /**********************************************************************/
                addRadialDimensionLarge(modelSpaceId, 3, 8, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a 2 Line Angular Dimension                                       */
                /**********************************************************************/
                add2LineAngularDimension(modelSpaceId, 3, 9, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add an ACIS Solid                                                  */
                /**********************************************************************/
                addACIS(modelSpaceId, 1, 5, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add an Image                                                       */
                /**********************************************************************/
                addImage(modelSpaceId, 4, 0, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add an Xref                                                        */
                /**********************************************************************/
                addXRef(modelSpaceId, 4, 1, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a Table                                                        */
                /**********************************************************************/
                addTable(modelSpaceId, odaBlock1Id, 4, 2, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a Diametric Dimension                                               */
                /**********************************************************************/
                addDiametricDimension(modelSpaceId, 4, 3, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a Shape                                                        */
                /**********************************************************************/
                addShape(modelSpaceId, 4, 4, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a MInsert                                                      */
                /**********************************************************************/
                addMInsert(modelSpaceId, 4, 5, odaLayer1Id, ttfStyleId, odaBlock1Id);

                /**********************************************************************/
                /* Add an Xline                                                       */
                /**********************************************************************/
                addXline(modelSpaceId, 4, 6, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add custom objects                                                 */
                /**********************************************************************/
                addCustomObjects(pDb);

                /**********************************************************************/
                /* Add Text with Field                                                */
                /**********************************************************************/
                addTextWithField(modelSpaceId, 5, 0, odaLayer1Id, shxTextStyleId, ttfStyleId);

                /**********************************************************************/
                /* Add Box                                                            */
                /**********************************************************************/
                addBox(modelSpaceId, 5, 2, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Frustum                                                        */
                /**********************************************************************/
                addFrustum(modelSpaceId, 5, 3, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Sphere                                                         */
                /**********************************************************************/
                addSphere(modelSpaceId, 5, 4, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Torus                                                          */
                /**********************************************************************/
                addTorus(modelSpaceId, 5, 5, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Wedge                                                          */
                /**********************************************************************/
                addWedge(modelSpaceId, 5, 6, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Region                                                         */
                /**********************************************************************/
                addRegion(modelSpaceId, 5, 7, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Extrusion                                                          */
                /**********************************************************************/
                addExtrusion(modelSpaceId, 6, 0, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Revolution                                                          */
                /**********************************************************************/
                addSolRev(modelSpaceId, 6, 1, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Helix                                                          */
                /**********************************************************************/
                addHelix(modelSpaceId, 6, 2, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add Dwf Underlay                                                   */
                /**********************************************************************/
                addDwfUnderlay(modelSpaceId, 6, 3, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add some MLeaders                                                  */
                /**********************************************************************/
                addMLeaders(modelSpaceId, 6, 4, odaLayer1Id, ttfStyleId);

                /**********************************************************************/
                /* Add a Layout                                                       */
                /**********************************************************************/
                addLayout(pDb);


                //// If preview bitmap is already available it can be specified to avoid wasting
                //// time on generating it by DD
                //const OdChar* pBmpFileName = "preview.bmp");
                //if(::odSystemServices().accessFile(pBmpFileName, Oda::kFileRead))
                //{
                //  OdRdFileBuf bmpFile(pBmpFileName);

                //  OdArray<OdUInt8> buf;
                //  buf.resize((unsigned)bmpFile.length());
                //  OdUInt8 * pData = buf.asArrayPtr(); 
                //  bmpFile.getBytes(pData, buf.length());
                //  // Get length taking care about big-endian
                //  unsigned int length = pData[2] + (pData[3] << 8);
                //  pData += 14;  // Skip BITMAPFILEHEADER
                //  pDb.setThumbnailBitmap(pData, length);
                //}
            }
        }

        void addCustomObjects(Database pDb)
        {
            //Open the main dictionary
            using (DBDictionary pMain = (DBDictionary)pDb.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite),
                // Create the new dictionary.
            pOdtDic = new DBDictionary())
            {
                // Add new dictionary to the main dictionary.
                ObjectId dicId = pMain.SetAt("DWGDIRECT_OBJECTS", pOdtDic);

                // Create a new xrecord object.
                using (Xrecord pXRec = new Xrecord())
                {
                    // Add the xrecord the owning dictionary.
                    ObjectId xrId = pOdtDic.SetAt("PROPERTIES_1", pXRec);

                    ResultBuffer pRb = new ResultBuffer();
                    pRb.Add(new TypedValue(1000, "Sample XRecord Data"));
                    pRb.Add(new TypedValue(40, 3.14159));
                    pRb.Add(new TypedValue(70, (short)312));

                    pXRec.Data = pRb;
                }
            }
        } //end addCustomObjects

        /************************************************************************/
        /* Add a Layer to the specified database                                */
        /*                                                                      */
        /* The symbol table and symbol table record are implicitly closed when  */
        /* this function returns.                                               */
        /************************************************************************/
        ObjectId addLayer(Database pDb, string name, short color, string linetype)
        {
            /**********************************************************************/
            /* Open the layer table                                               */
            /**********************************************************************/
            using (LayerTable pLayers = (LayerTable)pDb.LayerTableId.GetObject(OpenMode.ForWrite))
            {
                /**********************************************************************/
                /* Create a layer table record                                        */
                /**********************************************************************/
                using (LayerTableRecord pLayer = new LayerTableRecord())
                {
                    /**********************************************************************/
                    /* Layer must have a name before adding it to the table.              */
                    /**********************************************************************/
                    pLayer.Name = name;

                    /**********************************************************************/
                    /* Set the Color.                                                     */
                    /**********************************************************************/
                    pLayer.Color = Color.FromColorIndex(ColorMethod.ByAci, color);

                    /**********************************************************************/
                    /* Set the Linetype.                                                  */
                    /**********************************************************************/
                    using (LinetypeTable pLinetypes = (LinetypeTable)pDb.LinetypeTableId.GetObject(OpenMode.ForWrite))
                    {
                        ObjectId linetypeId = pLinetypes[linetype];
                        pLayer.LinetypeObjectId = linetypeId;
                    }

                    /**********************************************************************/
                    /* Add the record to the table.                                       */
                    /**********************************************************************/
                    return pLayers.Add(pLayer);
                }
            }
        }

        /************************************************************************/
        /* Add a Registered Application to the specified database               */
        /************************************************************************/
        bool addRegApp(Database pDb, string name)
        {
            return false;// pDb.newRegApp(name);
        }

        /************************************************************************/
        /* Add a Text Style to the specified database                           */
        /*                                                                      */
        /* The symbol table and symbol table record are implicitly closed when  */
        /* this function returns.                                               */
        /************************************************************************/
        ObjectId addStyle(Database pDb, string styleName, double textSize,
                          double xScale, double priorSize, double obliquing,
                          string fileName)
        {
            return addStyle(pDb, styleName, textSize,
                            xScale, priorSize, obliquing,
                            fileName, false);
        }

        ObjectId addStyle(Database pDb, string styleName, double textSize,
                          double xScale, double priorSize, double obliquing,
                          string fileName, bool isShapeFile)
        {
            return addStyle(pDb, styleName, textSize,
                            xScale, priorSize, obliquing,
                            fileName, isShapeFile, string.Empty);
        }

        ObjectId addStyle(Database pDb, string styleName, double textSize,
                          double xScale, double priorSize, double obliquing,
                          string fileName, bool isShapeFile, string ttFaceName)
        {
            return addStyle(pDb, styleName, textSize,
                            xScale, priorSize, obliquing,
                            fileName, isShapeFile, ttFaceName, false);
        }

        ObjectId addStyle(Database pDb, string styleName, double textSize,
                          double xScale, double priorSize, double obliquing,
                          string fileName, bool isShapeFile, string ttFaceName, bool bold)
        {
            return addStyle(pDb, styleName, textSize,
                            xScale, priorSize, obliquing,
                            fileName, isShapeFile, ttFaceName, bold,
                            false);
        }

        ObjectId addStyle(Database pDb, string styleName, double textSize,
                          double xScale, double priorSize, double obliquing,
                          string fileName, bool isShapeFile, string ttFaceName, bool bold,
                          bool italic)
        {
            return addStyle(pDb, styleName, textSize,
                            xScale, priorSize, obliquing,
                            fileName, isShapeFile, ttFaceName, bold,
                            italic, 0);
        }

        ObjectId addStyle(Database pDb, string styleName, double textSize,
                          double xScale, double priorSize, double obliquing,
                          string fileName, bool isShapeFile, string ttFaceName, bool bold,
                          bool italic, int charset)
        {
            return addStyle(pDb, styleName, textSize,
                            xScale, priorSize, obliquing,
                            fileName, isShapeFile, ttFaceName, bold,
                            italic, charset, 0);
        }

        ObjectId addStyle(Database pDb, string styleName, double textSize,
                          double xScale, double priorSize, double obliquing,
                          string fileName, bool isShapeFile, string ttFaceName, bool bold,
                          bool italic, int charset, int pitchAndFamily)
        {
            ObjectId styleId = ObjectId.Null;

            using (TextStyleTable pStyles = (TextStyleTable)pDb.TextStyleTableId.GetObject(OpenMode.ForWrite))
            {
                using (TextStyleTableRecord pStyle = new TextStyleTableRecord())
                {
                    // Name must be set before a table object is added to a table.  The
                    // isShapeFile flag must also be set (if true) before adding the object
                    // to the database.
                    pStyle.Name = styleName;
                    pStyle.IsShapeFile = isShapeFile;

                    // Add the object to the table.
                    styleId = pStyles.Add(pStyle);

                    // Set the remaining properties.
                    pStyle.TextSize = textSize;
                    pStyle.XScale = xScale;
                    pStyle.PriorSize = priorSize;
                    pStyle.ObliquingAngle = obliquing;
                    pStyle.FileName = fileName;
                    if (isShapeFile)
                        pStyle.PriorSize = 22.45;

                    if (!string.IsNullOrEmpty(ttFaceName))
                        pStyle.Font = new FontDescriptor(ttFaceName, bold, italic, charset, pitchAndFamily);

                    return styleId;
                }
            }
        }

        /************************************************************************/
        /* Add a Linetype to the specified database                             */
        /*                                                                      */
        /* The symbol table and symbol table record are implicitly closed when  */
        /* this function returns.                                               */
        /************************************************************************/
        ObjectId addLinetype(Database pDb, string name, string comments)
        {
            /**********************************************************************/
            /* Open the Linetype table                                            */
            /**********************************************************************/
            using (LinetypeTable pLinetypes = (LinetypeTable)pDb.LinetypeTableId.GetObject(OpenMode.ForWrite))
            {
                using (LinetypeTableRecord pLinetype = new LinetypeTableRecord())
                {
                    /**********************************************************************/
                    /* Linetype must have a name before adding it to the table.           */
                    /**********************************************************************/
                    pLinetype.Name = name;

                    /**********************************************************************/
                    /* Add the record to the table.                                       */
                    /**********************************************************************/
                    ObjectId linetypeId = pLinetypes.Add(pLinetype);

                    /**********************************************************************/
                    /* Add the Comments.                                                  */
                    /**********************************************************************/
                    pLinetype.Comments = comments;

                    return linetypeId;
                }
            }
        }

        /************************************************************************/
        /* Add Several linetypes to the specified database                      */
        /************************************************************************/
        void addLinetypes(Database pDb, ObjectId shapeStyleId, ObjectId txtStyleId)
        {
            /**********************************************************************/
            /* Continuous linetype                                                */
            /**********************************************************************/
            addLinetype(pDb, "Continuous2", "Solid Line");

            /**********************************************************************/
            /* Hidden linetype                                                    */
            /* This is not the standard Hidden linetype, but is used by examples  */
            /**********************************************************************/
            ObjectId ltId = addLinetype(pDb, "Hidden", "- - - - - - - - - - - - - - - - - - - - -");
            using (LinetypeTableRecord pLt = (LinetypeTableRecord)ltId.GetObject(OpenMode.ForWrite))
            {
                pLt.NumDashes = 2;
                pLt.PatternLength = 0.1875;
                pLt.SetDashLengthAt(0, 0.125);
                pLt.SetDashLengthAt(1, -0.0625);
            }

            /**********************************************************************/
            /* Linetype with text                                                 */
            /**********************************************************************/
            ltId = addLinetype(pDb, "HW_ODA", "__ HW __ OD __ HW __ OD __");
            using (LinetypeTableRecord pLt = (LinetypeTableRecord)ltId.GetObject(OpenMode.ForWrite))
            {
                pLt.NumDashes = 6;
                pLt.PatternLength = 1.8;
                pLt.SetDashLengthAt(0, 0.5);
                pLt.SetDashLengthAt(1, -0.2);
                pLt.SetDashLengthAt(2, -0.2);
                pLt.SetDashLengthAt(3, 0.5);
                pLt.SetDashLengthAt(4, -0.2);
                pLt.SetDashLengthAt(5, -0.2);

                pLt.SetShapeStyleAt(1, txtStyleId);
                pLt.SetShapeOffsetAt(1, new Vector2d(-0.1, -0.05));
                pLt.SetTextAt(1, "HW");
                pLt.SetShapeScaleAt(1, 0.5);

                pLt.SetShapeStyleAt(4, txtStyleId);
                pLt.SetShapeOffsetAt(4, new Vector2d(-0.1, -0.05));
                pLt.SetTextAt(4, "OD");
                pLt.SetShapeScaleAt(4, 0.5);
            }

            /**********************************************************************/
            /* ZIGZAG linetype                                                    */
            /**********************************************************************/
            ltId = addLinetype(pDb, "ZigZag", "/\\/\\/\\/\\/\\/\\/\\/\\");
            using (LinetypeTableRecord pLt = (LinetypeTableRecord)ltId.GetObject(OpenMode.ForWrite))
            {
                pLt.NumDashes = 4;
                pLt.PatternLength = 0.8001;
                pLt.SetDashLengthAt(0, 0.0001);
                pLt.SetDashLengthAt(1, -0.2);
                pLt.SetDashLengthAt(2, -0.4);
                pLt.SetDashLengthAt(3, -0.2);

                pLt.SetShapeStyleAt(1, shapeStyleId);
                pLt.SetShapeOffsetAt(1, new Vector2d(-0.2, 0.0));
                pLt.SetShapeNumberAt(1, 131); //ZIG shape
                pLt.SetShapeScaleAt(1, 0.2);

                pLt.SetShapeStyleAt(2, shapeStyleId);
                pLt.SetShapeOffsetAt(2, new Vector2d(0.2, 0.0));
                pLt.SetShapeNumberAt(2, 131); //ZIG shape
                pLt.SetShapeScaleAt(2, 0.2);
                pLt.SetShapeRotationAt(2, 3.1415926);
            }
        }

        //    /************************************************************************/
        //    /* Add a block definition to the specified database                     */
        //    /*                                                                      */
        //    /* Note that the BlockTable and BlockTableRecord are implicitly closed  */
        //    /* when before this function returns.                                   */
        //    /************************************************************************/
        //    ObjectId addBlock(Database pDb, 
        //                                    string name)
        //    {
        //      ObjectId            id;
        //      BlockTable       pTable  = pDb.getBlockTableId().GetObject(OpenMode.ForWrite);
        //      BlockTableRecord pRecord = BlockTableRecord();

        //      /**********************************************************************/
        //      /* Block must have a name before adding it to the table.              */
        //      /**********************************************************************/
        //      pRecord.setName(name);

        //      /**********************************************************************/
        //      /* Add the record to the table.                                       */
        //      /**********************************************************************/
        //      id = pTable.add(pRecord);
        //      return id;
        //    }

        /************************************************************************/
        /* Add a block reference to the specified BlockTableRecord              */
        /************************************************************************/
        ObjectId addInsert(BlockTableRecord bBTR, ObjectId btrId, double xscale, double yscale)
        {
            ObjectId brefId;
            /**********************************************************************/
            /* Add the block reference to the BlockTableRecord                    */
            /**********************************************************************/
            using (BlockReference pBlkRef = new BlockReference(Point3d.Origin, bBTR.ObjectId))
            {
                using (Database pDb = bBTR.Database)
                    pBlkRef.SetDatabaseDefaults(pDb);
                brefId = bBTR.AppendEntity(pBlkRef);

                /**********************************************************************/
                /* Set some properties                                                */
                /**********************************************************************/
                pBlkRef.BlockTableRecord = btrId;
                pBlkRef.ScaleFactors = new Scale3d(xscale, yscale, 1.0);
                return brefId;
            }
        }

        /************************************************************************/
        /* Add a text entity with the specified attributes to the specified     */
        /* BlockTableRecord                                                     */
        /************************************************************************/
        ObjectId addTextEnt(BlockTableRecord bBTR, Point3d position, Point3d ap,
                            string str, double height, TextHorizontalMode hMode, TextVerticalMode vMode,
                            ObjectId layerId, ObjectId styleId)
        {
            return addTextEnt(bBTR, position, ap, str, height, hMode, vMode, layerId, styleId, null);
        }

        ObjectId addTextEnt(BlockTableRecord bBTR, Point3d position, Point3d ap,
                            string str, double height, TextHorizontalMode hMode, TextVerticalMode vMode,
                            ObjectId layerId, ObjectId styleId, Group pGroup)
        {
            /**********************************************************************/
            /* Create the text object                                             */
            /**********************************************************************/
            using (DBText pText = new DBText())
            {
                /*实体Entity--例如圆、直线、矩形等，它们都具有一些共同的属性，包括颜色、线型、线型
                比例、可见性、图层等，当添加实体到块表记录时，如果没有明确指定这些属性，AutoCAD会自动调用
                SetDatabaseDefaults()函数来设置默认值 */
                using (Database pDb = bBTR.Database)
                    pText.SetDatabaseDefaults(pDb);
                ObjectId textId = bBTR.AppendEntity(pText);

                // Make the text annotative
                pText.Annotative = AnnotativeStates.True;

                /**********************************************************************/
                /* Add the text to the specified group                                */
                /**********************************************************************/
                if (pGroup != null)
                    pGroup.Append(textId);

                /**********************************************************************/
                /* Set some properties                                                */
                /**********************************************************************/
                pText.Position = position;
                pText.AlignmentPoint = ap;
                pText.Height = height;
                pText.WidthFactor = 1.0;
                pText.TextString = str;
                pText.HorizontalMode = hMode;
                pText.VerticalMode = vMode;

                /**********************************************************************/
                /* Set the text to the specified style                                */
                /**********************************************************************/
                if (!styleId.IsNull)
                    pText.TextStyleId = styleId;
                /**********************************************************************/
                /* Set the text to the specified layer                                */
                /**********************************************************************/
                if (!layerId.IsNull)
                    pText.SetLayerId(layerId, false);

                return textId;
            }
        }

        /************************************************************************/
        /* Add a point entity with the specified attributes to the specified    */
        /* BlockTableRecord                                                     */
        /************************************************************************/
        ObjectId addPointEnt(BlockTableRecord bBTR, Point3d point, ObjectId layerId, Group pGroup)
        {
            /**********************************************************************/
            /* Create the point object                                             */
            /**********************************************************************/
            using (DBPoint pPoint = new DBPoint())
            {
                /*实体Entity--例如圆、直线、矩形等，它们都具有一些共同的属性，包括颜色、线型、线型
                比例、可见性、图层等，当添加实体到块表记录时，如果没有明确指定这些属性，AutoCAD会自动调用
                SetDatabaseDefaults()函数来设置默认值 */
                using (Database pDb = bBTR.Database)
                    pPoint.SetDatabaseDefaults(pDb);
                ObjectId pointId = bBTR.AppendEntity(pPoint);

                /**********************************************************************/
                /* Set some properties                                                */
                /**********************************************************************/
                pPoint.Position = point;

                /**********************************************************************/
                /* Add the point to the specified group                               */
                /**********************************************************************/
                if (pGroup != null)
                {
                    pGroup.Append(pointId);
                }
                /**********************************************************************/
                /* Set the point to the specified layer                               */
                /**********************************************************************/
                if (!layerId.IsNull)
                {
                    pPoint.LayerId = layerId;
                }
                return pointId;
            }
        }

        /************************************************************************/
        /* Add some text entities to the specified BlockTableRecord             */
        /*                                                                      */
        /* The newly created entities are placed in a group                     */
        /************************************************************************/
        void addTextEnts(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            using (BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite))
            {
                // We want to place all text items into a newly created group, so
                // open the group dictionary here.

                /**********************************************************************/
                /* Open the Group Dictionary                                          */
                /**********************************************************************/
                using (DBDictionary pGroupDic = (DBDictionary)btrId.Database.GroupDictionaryId.GetObject(OpenMode.ForWrite))
                {
                    /**********************************************************************/
                    /* Create a new Group                                                 */
                    /**********************************************************************/
                    Group pGroup = new Group();

                    /**********************************************************************/
                    /* Add it to the Group Dictionary                                     */
                    /**********************************************************************/
                    pGroupDic.SetAt("OdaGroup", pGroup);

                    /**********************************************************************/
                    /* Set some properties                                                 */
                    /**********************************************************************/
                    pGroup.Name = "OdaGroup";
                    pGroup.Selectable = true;

                    /**********************************************************************/
                    /* Get the Lower-left corner of the box and its size                  */
                    /**********************************************************************/
                    Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                    double h = m_EntityBoxes.getHeight();
                    double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                    double dx = w / 16.0;
                    double dy = h / 12.0;

                    double textHeight = m_EntityBoxes.getHeight() / 12.0;

                    /**********************************************************************/
                    /* Add a label                                                        */
                    /**********************************************************************/
                    addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "TEXT",
                      m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                    /**********************************************************************/
                    /* Get the lower-left corner of the box                               */
                    /**********************************************************************/
                    point -= Vector3d.YAxis * h;

                    /**********************************************************************/
                    /* Add the text entities, and add them to the group                   */
                    /*                                                                    */
                    /* Show the relevant positions and alignment points                   */
                    /**********************************************************************/
                    Point3d position = point + new Vector3d(dx, dy * 9.0, 0.0);
                    addPointEnt(bBTR, position, layerId, pGroup);
                    addTextEnt(bBTR, position, position, "Left Text",
                      textHeight, TextHorizontalMode.TextLeft, TextVerticalMode.TextBase, ObjectId.Null, styleId, pGroup);

                    Point3d alignmentPoint = point + new Vector3d(w / 2.0, dy * 9.0, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Center Text",
                      textHeight, TextHorizontalMode.TextCenter, TextVerticalMode.TextBase, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(w - dx, dy * 9.0, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Right Text",
                      textHeight, TextHorizontalMode.TextRight, TextVerticalMode.TextBase, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(w / 2.0, dy * 8.0, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Middle Text",
                      textHeight, TextHorizontalMode.TextMid, TextVerticalMode.TextBase, ObjectId.Null, styleId, pGroup);

                    position = point + new Vector3d(dx, dy * 1, 0.0);
                    alignmentPoint = point + new Vector3d(w - dx, dy, 0.0);
                    addPointEnt(bBTR, position, layerId, pGroup);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, position, alignmentPoint, "Aligned Text",
                      textHeight, TextHorizontalMode.TextAlign, TextVerticalMode.TextBase, ObjectId.Null, styleId, pGroup);

                    position = point + new Vector3d(dx, dy * 5.5, 0.0);
                    alignmentPoint = point + new Vector3d(w - dx, dy * 5.5, 0.0);
                    addPointEnt(bBTR, position, layerId, pGroup);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, position, alignmentPoint, "Fit Text",
                      textHeight, TextHorizontalMode.TextFit, TextVerticalMode.TextBase, ObjectId.Null, styleId, pGroup);


                    /**********************************************************************/
                    /* Start a new box                                                    */
                    /**********************************************************************/
                    point = m_EntityBoxes.getBox(boxRow, boxCol + 1);

                    /**********************************************************************/
                    /* Add a label                                                        */
                    /**********************************************************************/
                    addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "TEXT",
                      m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                    /**********************************************************************/
                    /* Get the lower-left corner of the box                               */
                    /**********************************************************************/
                    point -= Vector3d.YAxis * h;
                    textHeight = h / 16.0;

                    /**********************************************************************/
                    /* Create a new anonymous Group                                       */
                    /**********************************************************************/
                    pGroup = new Group();

                    /**********************************************************************/
                    /* Add it to the Group Dictionary                                     */
                    /**********************************************************************/
                    pGroupDic.SetAt("*", pGroup);

                    /**********************************************************************/
                    /* Set some properties                                                 */
                    /**********************************************************************/
                    pGroup.Name = "*";
                    pGroup.SetAnonymous();
                    pGroup.Selectable = true;

                    /**********************************************************************/
                    /* Add the text entities, and add them to the group                   */
                    /*                                                                    */
                    /* Show the relevant positions and alignment points                   */
                    /**********************************************************************/
                    alignmentPoint = point + new Vector3d(dx, dy * 9.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Top Left",
                      textHeight, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(w / 2.0, dy * 9.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Top Center",
                      textHeight, TextHorizontalMode.TextCenter, TextVerticalMode.TextTop, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(w - dx, dy * 9.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Top Right",
                      textHeight, TextHorizontalMode.TextRight, TextVerticalMode.TextTop, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(dx, dy * 7.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Middle Left",
                      textHeight, TextHorizontalMode.TextLeft, TextVerticalMode.TextVerticalMid, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(w / 2.0, dy * 7.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Middle Center",
                      textHeight, TextHorizontalMode.TextCenter, TextVerticalMode.TextVerticalMid, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(w - dx, dy * 7.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Middle Right",
                      textHeight, TextHorizontalMode.TextRight, TextVerticalMode.TextVerticalMid, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(dx, dy * 5.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Baseline Left",
                      textHeight, TextHorizontalMode.TextLeft, TextVerticalMode.TextBase, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(w / 2.0, dy * 5.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Baseline Center",
                      textHeight, TextHorizontalMode.TextCenter, TextVerticalMode.TextBase, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(w - dx, dy * 5.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Baseline Right",
                      textHeight, TextHorizontalMode.TextRight, TextVerticalMode.TextBase, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(dx, dy * 3.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Bottom Left",
                      textHeight, TextHorizontalMode.TextLeft, TextVerticalMode.TextBottom, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(w / 2.0, dy * 3.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Bottom Center",
                      textHeight, TextHorizontalMode.TextCenter, TextVerticalMode.TextBottom, ObjectId.Null, styleId, pGroup);

                    alignmentPoint = point + new Vector3d(w - dx, dy * 3.5, 0.0);
                    addPointEnt(bBTR, alignmentPoint, layerId, pGroup);
                    addTextEnt(bBTR, alignmentPoint, alignmentPoint, "Bottom Right",
                      textHeight, TextHorizontalMode.TextRight, TextVerticalMode.TextBottom, ObjectId.Null, styleId, pGroup);
                }
            }
        }

        /************************************************************************/
        /* Append a PolygonMesh vertex to the specified PolygonMesh             */
        /************************************************************************/
        void appendPgMeshVertex(PolygonMesh pPgMesh, Point3d pos)
        {
            /**********************************************************************/
            /* Append a Vertex to the PolyFaceMesh                                */
            /**********************************************************************/
            using (PolygonMeshVertex pVertex = new PolygonMeshVertex())
            {
                pPgMesh.AppendVertex(pVertex);

                /**********************************************************************/
                /* Set the properties                                                 */
                /**********************************************************************/
                pVertex.Position = pos;
            }
        }

        /************************************************************************/
        /* Append a PolyFaceMesh vertex to the specified PolyFaceMesh           */
        /************************************************************************/
        void appendPfMeshVertex(PolyFaceMesh pMesh, double x, double y, double z)
        {
            /**********************************************************************/
            /* Append a MeshVertex to the PolyFaceMesh                            */
            /**********************************************************************/
            using (PolyFaceMeshVertex pVertex = new PolyFaceMeshVertex())
            {
                pMesh.AppendVertex(pVertex);

                /**********************************************************************/
                /* Set the properties                                                 */
                /**********************************************************************/
                pVertex.Position = new Point3d(x, y, z);
            }
        }

        /************************************************************************/
        /* Append a FaceRecord to the specified PolyFaceMesh                    */
        /************************************************************************/
        void appendFaceRecord(PolyFaceMesh pMesh, short i1, short i2, short i3, short i4)
        {
            /**********************************************************************/
            /* Append a FaceRecord to the PolyFaceMesh                            */
            /**********************************************************************/
            using (FaceRecord pFr = new FaceRecord())
            {
                pMesh.AppendFaceRecord(pFr);

                /**********************************************************************/
                /* Set the properties                                                 */
                /**********************************************************************/
                pFr.SetVertexAt(0, i1);
                pFr.SetVertexAt(1, i2);
                pFr.SetVertexAt(2, i3);
                pFr.SetVertexAt(3, i4);
            }
        }

        /************************************************************************/
        /* Add an MLine Style to the specified database                         */
        /************************************************************************/
        ObjectId addMLineStyle(Database pDb, string name, string desc)
        {
            /**********************************************************************/
            /* Open the MLineStyle dictionary                                     */
            /**********************************************************************/
            using (DBDictionary pMLDic = (DBDictionary)pDb.MLStyleDictionaryId.GetObject(OpenMode.ForWrite))
            {
                /**********************************************************************/
                /* Create an Mline Style                                              */
                /**********************************************************************/
                using (MlineStyle pStyle = new MlineStyle())
                {
                    /**********************************************************************/
                    /* Set some parameters                                                */
                    /**********************************************************************/
                    pStyle.Name = name;
                    pStyle.Description = desc;
                    pStyle.StartAngle = OdaToRadian(105.0);
                    pStyle.EndAngle = OdaToRadian(75.0);
                    pStyle.ShowMiters = true;
                    pStyle.StartSquareCap = true;
                    pStyle.EndSquareCap = true;

                    /**********************************************************************/
                    /* Get the object ID of the desired linetype                          */
                    /**********************************************************************/
                    using (LinetypeTable pLtTable = (LinetypeTable)pDb.LinetypeTableId.GetObject(OpenMode.ForRead))
                    {
                        ObjectId linetypeId = pLtTable["Hidden"];

                        /**********************************************************************/
                        /* Add some elements                                                  */
                        /**********************************************************************/
                        pStyle.Elements.Add(new MlineStyleElement(0.1, Color.FromRgb(255, 0, 0), linetypeId), true);
                        pStyle.Elements.Add(new MlineStyleElement(0.0, Color.FromRgb(0, 255, 0), linetypeId), true);
                    }
                    /**********************************************************************/
                    /* Update the MLine dictionary                                        */
                    /**********************************************************************/
                    return pMLDic.SetAt(name, pStyle);
                }
            }
        }

        /************************************************************************/
        /* Add a Material to the specified database                             */
        /************************************************************************/
        ObjectId addMaterial(Database pDb, string name, string desc)
        {
            /**********************************************************************/
            /* Open the Material dictionary                                     */
            /**********************************************************************/
            using (DBDictionary pMatDic = (DBDictionary)pDb.MaterialDictionaryId.GetObject(OpenMode.ForWrite))
            {
                /**********************************************************************/
                /* Create a Material                                                  */
                /**********************************************************************/
                using (Material pMaterial = new Material())
                {
                    /**********************************************************************/
                    /* Set some parameters                                                */
                    /**********************************************************************/
                    pMaterial.Name = name;
                    pMaterial.Description = desc;

                    MaterialMap materialMap = new MaterialMap(Source.File, new MaterialTexture(), 0, null);
                    using (MaterialColor materialColor = new MaterialColor(Method.Override, 0.75, new EntityColor(192, 32, 255)))
                    {
                        pMaterial.Ambient = materialColor;
                        pMaterial.Bump = materialMap;
                        pMaterial.Diffuse = new MaterialDiffuseComponent(materialColor, materialMap);
                    }
                    pMaterial.Opacity = new MaterialOpacityComponent(1.0, materialMap);
                    pMaterial.Reflection = materialMap;
                    pMaterial.Refraction = new MaterialRefractionComponent(1.0, materialMap);
                    pMaterial.Translucence = 0.0;
                    pMaterial.SelfIllumination = 0.0;
                    pMaterial.Reflectivity = 0.0;
                    pMaterial.Mode = Mode.Realistic;
                    pMaterial.ChannelFlags = ChannelFlags.None;
                    pMaterial.IlluminationModel = IlluminationModel.BlinnShader;

                    using (MaterialColor materialColor = new MaterialColor(Method.Override, 1.0, new EntityColor(255, 255, 255)))
                    {
                        pMaterial.Specular = new MaterialSpecularComponent(materialColor, materialMap, 0.67);
                    }
                    /**********************************************************************/
                    /* Update the Material dictionary                                        */
                    /**********************************************************************/
                    return pMatDic.SetAt(name, pMaterial);
                }
            }
        }

        /************************************************************************/
        /* Add some lines to the specified BlockTableRecord                     */
        /************************************************************************/
        void addLines(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            using (BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite))
            {
                /**********************************************************************/
                /* Get the origin and size of the box                                 */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                //  double      w     = m_EntityBoxes.getWidth(boxRow, boxCol);
                //  double      h     = m_EntityBoxes.getHeight();

                /**********************************************************************/
                /* Add a label                                                        */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "LINEs", m_textSize,
                            TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the center of the box                                          */
                /**********************************************************************/
                point = m_EntityBoxes.getBoxCenter(0, 0);

                /**********************************************************************/
                /* Add the lines that describe a 12 pointed star                      */
                /**********************************************************************/
                Vector3d toStart = Vector3d.XAxis;
                using (Database pDb = bBTR.Database)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        Line pLine = new Line();
                        /*实体Entity--例如圆、直线、矩形等，它们都具有一些共同的属性，包括颜色、线型、线型
                        比例、可见性、图层等，当添加实体到块表记录时，如果没有明确指定这些属性，AutoCAD会自动调用
                        SetDatabaseDefaults()函数来设置默认值 */
                        pLine.SetDatabaseDefaults(pDb);
                        bBTR.AppendEntity(pLine);
                        pLine.StartPoint = point + toStart;
                        pLine.EndPoint = point + toStart.RotateBy(OdaToRadian(160.0), Vector3d.ZAxis);
                    }
                }
            }
        }

        /************************************************************************/
        /* Add a 2D (heavy) polyline to the specified BlockTableRecord          */
        /************************************************************************/
        void add2dPolyline(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            using (BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite))
            {
                using (Database pDb = bBTR.Database)
                {
                    /**********************************************************************/
                    /* Get the origin and size of the box                                 */
                    /**********************************************************************/
                    Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                    double w = m_EntityBoxes.getWidth(boxRow, boxCol);
                    double h = m_EntityBoxes.getHeight();

                    /**********************************************************************/
                    /* Add a label                                                        */
                    /**********************************************************************/
                    addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "2D POLYLINE", m_textSize,
                      TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                    /**********************************************************************/
                    /* Add a 2dPolyline to the database                                   */
                    /**********************************************************************/
                    Polyline2d pPline = new Polyline2d();
                    /*实体Entity--例如圆、直线、矩形等，它们都具有一些共同的属性，包括颜色、线型、线型
                    比例、可见性、图层等，当添加实体到块表记录时，如果没有明确指定这些属性，AutoCAD会自动调用
                    SetDatabaseDefaults()函数来设置默认值 */
                    pPline.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(pPline);

                    /**********************************************************************/
                    /* Add the vertices                                                   */
                    /**********************************************************************/
                    Point3d pos = point;
                    pos = pos + new Vector3d(w / 8, h / 8 - h, 0);

                    double[,] width = new double[2, 4]
      {
        {0.0, w/12, w/4, 0.0},
        {w/4, w/12, 0.0, 0.0}
      };

                    for (int i = 0; i < 4; i++)
                    {
                        Vertex2d pVertex = new Vertex2d();
                        /*实体Entity--例如圆、直线、矩形等，它们都具有一些共同的属性，包括颜色、线型、线型
                        比例、可见性、图层等，当添加实体到块表记录时，如果没有明确指定这些属性，AutoCAD会自动调用
                        SetDatabaseDefaults()函数来设置默认值 */
                        pVertex.SetDatabaseDefaults(pDb);
                        pPline.AppendVertex(pVertex);
                        pVertex.Position = pos;
                        pos = pos + new Vector3d(w / 4.0, h / 4.0, 0);
                        pVertex.StartWidth = width[0, i];
                        pVertex.EndWidth = width[1, i];
                    }
                }
            }
        }

        /************************************************************************/
        /* Add a 3D polyline to the specified BlockTableRecord                  */
        /************************************************************************/
        void add3dPolyline(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            using (BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite))
            {
                using (Database pDb = bBTR.Database)
                {
                    /**********************************************************************/
                    /* Get the origin and size of the box                                 */
                    /**********************************************************************/
                    Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                    double w = m_EntityBoxes.getWidth(boxRow, boxCol);
                    //  double      h     = m_EntityBoxes.getHeight();

                    /**********************************************************************/
                    /* Add a label                                                        */
                    /**********************************************************************/
                    addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "3D POLYLINE",
                      m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                    /**********************************************************************/
                    /* Add a 3dPolyline to the database                                   */
                    /**********************************************************************/
                    Polyline3d pPline = new Polyline3d();
                    pPline.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(pPline);

                    /**********************************************************************/
                    /* Add the vertices                                                   */
                    /**********************************************************************/
                    Point3d pos = point;
                    Point3d center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);

                    double radius = w * 3.0 / 8.0;
                    double height = 0.0;
                    double theta = 0.0;

                    int turns = 4;
                    int segs = 16;
                    int points = segs * turns;

                    double deltaR = radius / points;
                    double deltaTheta = 2 * Math.PI / segs;
                    double deltaH = 2 * radius / points;

                    Vector3d vec = new Vector3d(radius, 0, 0);

                    for (int i = 0; i < points; i++)
                    {
                        using (PolylineVertex3d pVertex = new PolylineVertex3d())
                        {
                            pVertex.SetDatabaseDefaults(pDb);
                            pPline.AppendVertex(pVertex);
                            pVertex.Position = center + vec;
                        }
                        radius -= deltaR;
                        height += deltaH;
                        theta += deltaTheta;
                        vec = new Vector3d(radius, 0, height).RotateBy(theta, Vector3d.ZAxis);
                    }
                }
            }
        }
        /************************************************************************/
        /* Add MText to the specified BlockTableRecord                          */
        /************************************************************************/
        void addMText(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            using (BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite))
            {
                /**********************************************************************/
                /* Get the origin and size of the box                                 */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();

                /**********************************************************************/
                /* Add a label                                                        */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "MTEXT",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Add MText to the database                                          */
                /**********************************************************************/
                using (MText pMText = new MText())
                {
                    using (Database pDb = bBTR.Database)
                        pMText.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(pMText);

                    /**********************************************************************/
                    /* Set some properties                                                */
                    /**********************************************************************/
                    pMText.Location = point + new Vector3d(w / 8.0, -h * 2.0 / 8.0, 0);
                    pMText.TextHeight = 0.4;
                    pMText.Attachment = AttachmentPoint.TopLeft;
                    pMText.Contents = "Sample {\\C1;MTEXT} created by {\\C5;OdWriteEx}";
                    pMText.Width = w * 6.0 / 8.0;
                    pMText.TextStyle = styleId;
                }
            }
        }

        /************************************************************************/
        /* Add a Block Reference to the specified BlockTableRecord              */
        /************************************************************************/
        void addBlockRef(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId, ObjectId insertId)
        {
            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            using (BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite))
            {
                using (Database pDb = bBTR.Database)
                {
                    /**********************************************************************/
                    /* Get the origin and size of the box                                 */
                    /**********************************************************************/
                    Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                    //  double      w     = m_EntityBoxes.getWidth(boxRow, boxCol);
                    //  double      h     = m_EntityBoxes.getHeight();

                    /**********************************************************************/
                    /* Add a label                                                        */
                    /**********************************************************************/
                    addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "INSERT",
                      m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                    /**********************************************************************/
                    /* Insert the Block                                                   */
                    /**********************************************************************/
                    ObjectId bklRefId = addInsert(bBTR, insertId, 1.0, 1.0);

                    /**********************************************************************/
                    /* Open the insert                                                    */
                    /**********************************************************************/
                    using (BlockReference pBlkRef = (BlockReference)bklRefId.GetObject(OpenMode.ForWrite))
                    {
                        /**********************************************************************/
                        /* Create a transformation matrix for the block and attributes        */
                        /**********************************************************************/
                        Point3d insPoint = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                        Matrix3d blkXfm = Matrix3d.Displacement(insPoint.GetAsVector());
                        pBlkRef.TransformBy(blkXfm);

                        /**********************************************************************/
                        /* Scan the block definition for non-constant attribute definitions   */
                        /* and use them as templates for attributes                           */
                        /**********************************************************************/
                        using (BlockTableRecord pBlockDef = (BlockTableRecord)insertId.GetObject(OpenMode.ForWrite))
                        {
                            foreach (ObjectId idEnt in pBlockDef)
                            {
                                using (AttributeDefinition pAttDef = idEnt.GetObject(OpenMode.ForWrite) as AttributeDefinition)
                                {
                                    if (pAttDef != null && !pAttDef.Constant)
                                    {
                                        using (AttributeReference pAtt = new AttributeReference())
                                        {
                                            pAtt.SetDatabaseDefaults(pDb);
                                            pBlkRef.AttributeCollection.AppendAttribute(pAtt);
                                            pAtt.SetPropertiesFrom(pAttDef);
                                            pAtt.AlignmentPoint = pAttDef.AlignmentPoint;
                                            pAtt.Height = pAttDef.Height;
                                            pAtt.HorizontalMode = pAttDef.HorizontalMode;
                                            pAtt.Normal = pAttDef.Normal;
                                            pAtt.Oblique = pAttDef.Oblique;
                                            pAtt.Position = pAttDef.Position;
                                            pAtt.Rotation = pAttDef.Rotation;
                                            pAtt.TextString = pAttDef.TextString;
                                            pAtt.TextStyleId = pAttDef.TextStyleId;
                                            pAtt.WidthFactor = pAttDef.WidthFactor;

                                            /******************************************************************/
                                            /* Specify a new value for the attribute                          */
                                            /******************************************************************/
                                            pAtt.TextString = "The Value";

                                            /******************************************************************/
                                            /* Transform it as the block was transformed                      */
                                            /******************************************************************/
                                            pAtt.TransformBy(blkXfm);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /************************************************************************/
        /* Add a MInsert to the specified BlockTableRecord                      */
        /************************************************************************/
        void addMInsert(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId, ObjectId insertId)
        {
            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the origin and size of the box                                 */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();

                /**********************************************************************/
                /* Add a label                                                        */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "MInsert",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Add MInsert to the database                                        */
                /**********************************************************************/
                MInsertBlock pMInsert = new MInsertBlock();
                pMInsert.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pMInsert);

                /**********************************************************************/
                /* Set some Parameters                                                */
                /**********************************************************************/
                pMInsert.BlockTableRecord = insertId;
                Point3d insPnt = point + new Vector3d(w * 2.0 / 8.0, h * 2.0 / 8.0, 0.0);
                pMInsert.Position = insPnt;
                pMInsert.ScaleFactors = new Scale3d(2.0 / 8.0);
                pMInsert.Rows = 2;
                pMInsert.Columns = 3;
                pMInsert.RowSpacing = h * 4.0 / 8.0;
                pMInsert.ColumnSpacing = w * 2.0 / 8.0;
            }
        }

        /************************************************************************/
        /* Add a PolyFaceMesh to the specified BlockTableRecord                 */
        /************************************************************************/
        void addPolyFaceMesh(ObjectId btrId, int boxRow, int boxCol,
                             ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the origin and size of the box                                 */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();

                /**********************************************************************/
                /* Add a label                                                        */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "PolyFaceMesh",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Add a PolyFaceMesh to the database                                 */
                /**********************************************************************/
                PolyFaceMesh pMesh = new PolyFaceMesh();
                pMesh.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pMesh);

                /**********************************************************************/
                /* Add the faces and vertices that define a pup tent                  */
                /**********************************************************************/
                double dx = w * 3.0 / 8.0;
                double dy = h * 3.0 / 8.0;
                double dz = dy;

                Point3d center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);

                appendPfMeshVertex(pMesh, center.X + dx, center.Y + dy, 0);
                appendPfMeshVertex(pMesh, center.X + 0, center.Y + dy, center.Z + dz);
                appendPfMeshVertex(pMesh, center.X - dx, center.Y + dy, 0);
                appendPfMeshVertex(pMesh, center.X - dx, center.Y - dy, 0);
                appendPfMeshVertex(pMesh, center.X + 0, center.Y - dy, center.Z + dz);
                appendPfMeshVertex(pMesh, center.X + dx, center.Y - dy, 0);

                appendFaceRecord(pMesh, 1, 2, 5, 6);
                appendFaceRecord(pMesh, 2, 3, 4, 5);
                appendFaceRecord(pMesh, 6, 5, 4, 0);
                appendFaceRecord(pMesh, 3, 2, 1, 0);
            }
        }

        /************************************************************************/
        /* Add PolygonMesh to the specified BlockTableRecord                    */
        /************************************************************************/
        void addPolygonMesh(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);

            /**********************************************************************/
            /* Get the origin and size of the box                                 */
            /**********************************************************************/
            Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
            double w = m_EntityBoxes.getWidth(boxRow, boxCol);
            double h = m_EntityBoxes.getHeight();

            /**********************************************************************/
            /* Add a label                                                        */
            /**********************************************************************/
            addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "PolygonMesh", m_textSize,
              TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

            /**********************************************************************/
            /* Add a PolygonMesh to the database                                 */
            /**********************************************************************/
            PolygonMesh pMesh = new PolygonMesh();
            using (Database pDb = bBTR.Database)
                pMesh.SetDatabaseDefaults(pDb);
            bBTR.AppendEntity(pMesh);

            /**********************************************************************/
            /* Define the size of the mesh                                        */
            /**********************************************************************/
            short mSize = 16, nSize = 4;
            pMesh.MSize = mSize;
            pMesh.NSize = nSize;

            /**********************************************************************/
            /* Define a profile                                                   */
            /**********************************************************************/
            double dx = w * 3.0 / 8.0;
            double dy = h * 3.0 / 8.0;

            Vector3d[] vectors = { new Vector3d(0,  -dy, 0),
                             new Vector3d(dx, -dy, 0),
                             new Vector3d(dx,  dy, 0),
                             new Vector3d(0,   dy, 0)};

            Point3d center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);

            /**********************************************************************/
            /* Append the vertices to the mesh                                    */
            /**********************************************************************/
            for (int i = 0; i < mSize; i++)
            {
                for (int j = 0; j < nSize; j++)
                {
                    appendPgMeshVertex(pMesh, center + vectors[j]);
                    vectors[j] = vectors[j].RotateBy(OdaToRadian(360.0 / mSize), Vector3d.YAxis);
                }
            }
            pMesh.MakeMClosed();
        }

        /************************************************************************/
        /* Add some curves to the specified BlockTableRecord                    */
        /************************************************************************/
        void addCurves(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Get the origin and size of the box                                 */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();

                /**********************************************************************/
                /* Create a Circle                                                    */
                /**********************************************************************/
                Circle pCircle = new Circle();
                /*实体Entity--例如圆、直线、矩形等，它们都具有一些共同的属性，包括颜色、线型、线型
                比例、可见性、图层等，当添加实体到块表记录时，如果没有明确指定这些属性，AutoCAD会自动调用
                SetDatabaseDefaults()函数来设置默认值 */
                pCircle.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pCircle);

                Point3d center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                center -= new Vector3d(w * 2.5 / 8.0, 0, 0);
                pCircle.Center = center;
                pCircle.Radius = w * 1.0 / 8.0;

                /**********************************************************************/
                /* Add a Hyperlink to the Circle                                      */
                /**********************************************************************/
                HyperLinkCollection urls = pCircle.Hyperlinks;
                HyperLink hl = new HyperLink();
                hl.Name = "http://forum.opendesign.com/forumdisplay.php?s=&forumid=17";
                hl.Description = "Open Design Alliance Forum > DWGdirect, C++ version";
                urls.Add(hl);

                /**********************************************************************/
                /* Add a label                                                        */
                /**********************************************************************/
                double textY = point.Y - m_textSize / 2.0;

                addTextEnt(bBTR, new Point3d(center.X, textY, 0), new Point3d(center.X, textY, 0),
                  "CIRCLE", m_textSize, TextHorizontalMode.TextCenter, TextVerticalMode.TextTop, layerId, styleId);

                addTextEnt(bBTR, new Point3d(center.X, textY - 1.6 * m_textSize, 0), new Point3d(center.X, textY - 1.6 * m_textSize, 0),
                  "w/Hyperlink", m_textSize, TextHorizontalMode.TextCenter, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Create an Arc                                                      */
                /**********************************************************************/
                Arc pArc = new Arc();
                pArc.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pArc);

                pArc.Radius = w * 1.0 / 8.0;

                center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);

                center += Vector3d.YAxis * pArc.Radius / 2.0;

                pArc.Center = center;
                pArc.StartAngle = OdaToRadian(0.0);
                pArc.EndAngle = OdaToRadian(180.0);

                /**********************************************************************/
                /* Add a label                                                        */
                /**********************************************************************/
                addTextEnt(bBTR, new Point3d(center.X, textY, 0), new Point3d(center.X, textY, 0),
                  "ARC", m_textSize, TextHorizontalMode.TextCenter, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Add an Ellipse                                                     */
                /**********************************************************************/
                Ellipse pEllipse = new Ellipse();
                pEllipse.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pEllipse);

                double majorRadius = w * 1.0 / 8.0;
                double radiusRatio = 0.25;

                center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                center += new Vector3d(w * 2.5 / 8.0, majorRadius, 0);

                Vector3d majorAxis = new Vector3d(majorRadius, 0.0, 0.0);
                majorAxis = majorAxis.RotateBy(OdaToRadian(30.0), Vector3d.ZAxis);

                pEllipse.Set(center, Vector3d.ZAxis, majorAxis, radiusRatio, 0, 2 * Math.PI);

                /**********************************************************************/
                /* Add a label                                                        */
                /**********************************************************************/
                addTextEnt(bBTR, new Point3d(center.X, textY, 0), new Point3d(center.X, textY, 0),
                  "ELLIPSE", m_textSize, TextHorizontalMode.TextCenter, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Add a Point                                                        */
                /**********************************************************************/
                DBPoint pPoint = new DBPoint();
                pPoint.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pPoint);

                center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                center -= Vector3d.YAxis * h * 1.0 / 8.0;

                pPoint.Position = center;

                /**********************************************************************/
                /* Add a label                                                        */
                /**********************************************************************/
                center += Vector3d.YAxis * h * 1.0 / 8.0;
                addTextEnt(bBTR, center, center, "POINT", m_textSize, TextHorizontalMode.TextCenter, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Set the point display mode so we can see it                        */
                /**********************************************************************/
                pDb.Pdmode = 3;
                pDb.Pdsize = 0.1;
            }
        }
        /************************************************************************/
        /* Add a tolerance to the specified BlockTableRecord                    */
        /************************************************************************/
        void addTolerance(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Get the origin and size of the box                                 */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();

                /**********************************************************************/
                /* Add a label                                                        */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset,
                    "TOLERANCE", m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Add a Frame Control Feature (Tolerance) to the database            */
                /**********************************************************************/
                FeatureControlFrame pTol = new FeatureControlFrame();
                pTol.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pTol);

                /**********************************************************************/
                /* Set the properties                                                 */
                /**********************************************************************/
                point += Vector3d.XAxis * w / 6.0;
                point -= Vector3d.YAxis * h / 4.0;
                pTol.Location = point;
                pTol.Text = "{\\Fgdt;r}%%v{\\Fgdt;n}3.2{\\Fgdt;m}%%v%%v%%v%%v";
            }
        }

        /************************************************************************/
        /* Add some leaders the specified BlockTableRecord                      */
        /************************************************************************/
        void addLeaders(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Get the origin and size of the box                                 */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();

                /**********************************************************************/
                /* Add a label                                                        */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "LEADERs",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Define an annotation block -- A circle with radius 0.5             */
                /**********************************************************************/
                BlockTable pBlocks = (BlockTable)pDb.BlockTableId.GetObject(OpenMode.ForWrite);
                BlockTableRecord pAnnoBlock = new BlockTableRecord();
                pAnnoBlock.Name = "AnnoBlock";
                ObjectId annoBlockId = pBlocks.Add(pAnnoBlock);

                Circle pCircle = new Circle();
                pCircle.SetDatabaseDefaults(pDb);
                pAnnoBlock.AppendEntity(pCircle);
                Point3d center = new Point3d(0.5, 0, 0);
                pCircle.Center = center;
                pCircle.Radius = 0.5;

                /**********************************************************************/
                /* Add a leader with database defaults to the database                */
                /**********************************************************************/
                Leader pLeader = new Leader();
                pLeader.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pLeader);
                /**********************************************************************/
                /* Add the vertices                                                   */
                /**********************************************************************/
                point += new Vector3d(w * 1.0 / 8.0, -(h * 3.0 / 8.0), 0);
                pLeader.AppendVertex(point);

                point += new Vector3d(w * 2.0 / 8.0, h * 1.0 / 8.0, 0);
                pLeader.AppendVertex(point);

                /**********************************************************************/
                /* Insert the annotation                                              */
                /**********************************************************************/
                BlockReference pBlkRef = new BlockReference(point, bBTR.ObjectId);
                pBlkRef.BlockTableRecord = annoBlockId;
                pBlkRef.ScaleFactors = new Scale3d(0.375, 0.375, 0.375);
                bBTR.AppendEntity(pBlkRef);

                /**********************************************************************/
                /* Attach the Block Reference as annotation to the Leader             */
                /**********************************************************************/
                pLeader.Annotation = pBlkRef.ObjectId;

                /**********************************************************************/
                /* Add a leader with database defaults to the database                */
                /**********************************************************************/
                pLeader = new Leader();
                pLeader.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pLeader);

                /**********************************************************************/
                /* Add the vertices                                                   */
                /**********************************************************************/
                point = m_EntityBoxes.getBox(boxRow, boxCol);
                point += new Vector3d(w * 1.0 / 8.0, -(h * 5.0 / 8.0), 0);
                pLeader.AppendVertex(point);

                point += new Vector3d(w * 1.0 / 8.0, h * 1.0 / 8.0, 0);
                pLeader.AppendVertex(point);

                point += Vector3d.XAxis * w * 1.0 / 8;
                /**********************************************************************/
                /* Set the arrowhead                                                  */
                /**********************************************************************/
                // TODO: pLeader.Dimldrblk = "DOT";

                /**********************************************************************/
                /* Create MText at a 30?angle                                        */
                /**********************************************************************/
                MText pMText = new MText();
                pMText.SetDatabaseDefaults(pDb);
                ObjectId mTextId = bBTR.AppendEntity(pMText);
                double textHeight = 0.15;
                double textWidth = 1.0;
                pMText.Location = point;
                pMText.Rotation = OdaToRadian(10.0);
                pMText.TextHeight = textHeight;
                pMText.Width = textWidth;
                pMText.Attachment = AttachmentPoint.MiddleLeft;
                pMText.Contents = "MText";
                pMText.TextStyle = styleId;

                /**********************************************************************/
                /* Set a background color                                             */
                /**********************************************************************/
                Color cBackground = Color.FromRgb(255, 255, 0);
                pMText.BackgroundFillColor = cBackground;
                pMText.BackgroundFill = true;
                pMText.BackgroundScaleFactor = 2;

                /**********************************************************************/
                /* Attach the MText as annotation to the Leader                       */
                /**********************************************************************/
                pLeader.Annotation = mTextId;

                /**********************************************************************/
                /* Add a leader with database defaults to the database                */
                /**********************************************************************/
                pLeader = new Leader();
                bBTR.AppendEntity(pLeader);
                pLeader.SetDatabaseDefaults(pDb);

                /**********************************************************************/
                /* Add the vertices                                                   */
                /**********************************************************************/
                point = m_EntityBoxes.getBox(boxRow, boxCol);
                point += new Vector3d(w * 1.0 / 8.0, -(h * 7.0 / 8.0), 0);
                pLeader.AppendVertex(point);

                point += new Vector3d(w * 1.0 / 8.0, h * 1.0 / 8.0, 0);
                pLeader.AppendVertex(point);

                /**********************************************************************/
                /* Create a Frame Control Feature (Tolerance)                         */
                /**********************************************************************/
                FeatureControlFrame pTol = new FeatureControlFrame();
                pTol.SetDatabaseDefaults(pDb);
                pTol.Location = point;
                pTol.Text = "{\\Fgdt;r}%%v{\\Fgdt;n}3.2{\\Fgdt;m}%%v%%v%%v%%v";

                /**********************************************************************/
                /* Attach the FCF as annotation to the Leader                         */
                /**********************************************************************/
                pLeader.Annotation = bBTR.AppendEntity(pTol);
            }
        }


        /************************************************************************/
        /* Add some MLeaders the specified BlockTableRecord                     */
        /************************************************************************/
        void addMLeaders(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            int llIndex;

            /**********************************************************************/
            /* Open the Block Table Record                                        */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Get the origin and size of the box                                 */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();

                /**********************************************************************/
                /* Add a label                                                        */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "MLeaders",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Add a MLeader with database defaults to the database               */
                /**********************************************************************/
                MLeader pMLeader = new MLeader();
                pMLeader.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pMLeader);

                /**********************************************************************/
                /* Add the vertices                                                   */
                /**********************************************************************/
                MText pMText = new MText();
                pMText.SetDatabaseDefaults(pDb);
                pMLeader.EnableFrameText = true;
                pMText.Contents = "MText";

                double textHeight = 0.15;
                double textWidth = 1.0;

                point += new Vector3d(w * 3.0 / 8.0, -h * 1.0 / 6.0, 0);
                pMText.Location = point;
                //  pMText.setRotation(OdaToRadian(10.0));
                pMText.TextHeight = textHeight;
                pMText.Width = textWidth;
                pMText.Attachment = AttachmentPoint.MiddleFit;
                pMText.TextStyle = styleId;
                pMLeader.MText = pMText;
                pMLeader.DoglegLength = 0.18;

                point -= new Vector3d(w * 2.0 / 8.0, h * 1.0 / 8.0, 0);
                llIndex = pMLeader.AddLeaderLine(point);

                point += Vector3d.XAxis * w * 1.0 / 8.0;
                //  point.y -= h * 3.0 / 8.0;
                llIndex = pMLeader.AddLeaderLine(point);
                point += new Vector3d(w * 1.0 / 6.0, -h * 1.0 / 8.0, 0);
                pMLeader.AddFirstVertex(llIndex, point);

                point += new Vector3d(w * 3.0 / 8.0, -h * 1.0 / 8.0, 0);
                llIndex = pMLeader.AddLeaderLine(point);
            }
        }

        /************************************************************************/
        /* Add a Block Definition to the specified database                     */
        /************************************************************************/
        ObjectId addBlockDef(Database pDb, string name, int boxRow, int boxCol)
        {
            /**********************************************************************/
            /* Open the block table                                               */
            /**********************************************************************/
            BlockTable pBlocks = (BlockTable)pDb.BlockTableId.GetObject(OpenMode.ForWrite);

            /**********************************************************************/
            /* Create a BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = new BlockTableRecord();

            /**********************************************************************/
            /* Block must have a name before adding it to the table.              */
            /**********************************************************************/
            bBTR.Name = name;

            /**********************************************************************/
            /* Add the record to the table.                                       */
            /**********************************************************************/
            ObjectId btrId = pBlocks.Add(bBTR);
            //  double h = m_EntityBoxes.getHeight();
            double w = m_EntityBoxes.getWidth(boxRow, boxCol);

            /**********************************************************************/
            /* Add a Circle                                                       */
            /**********************************************************************/
            Point3d center = new Point3d(-(w * 2.5 / 8.0), 0, 0);

            Circle pCircle = new Circle(center, Vector3d.ZAxis, w * 1.0 / 8.0);
            pCircle.SetDatabaseDefaults(pDb);
            bBTR.AppendEntity(pCircle);

            /**********************************************************************/
            /* Add an Arc                                                         */
            /**********************************************************************/
            Arc pArc = new Arc();
            pArc.SetDatabaseDefaults(pDb);
            bBTR.AppendEntity(pArc);

            pArc.Radius = w * 1.0 / 8.0;
            center = new Point3d(0, -pArc.Radius / 2.0, 0);

            pArc.Center = center;
            pArc.StartAngle = 0;
            pArc.EndAngle = Math.PI;

            /**********************************************************************/
            /* Add an Ellipse                                                     */
            /**********************************************************************/
            Ellipse pEllipse = new Ellipse();
            pEllipse.SetDatabaseDefaults(pDb);
            bBTR.AppendEntity(pEllipse);

            center = new Point3d(w * 2.5 / 8.0, 0, 0);

            double majorRadius = w * 1.0 / 8.0;
            Vector3d majorAxis = new Vector3d(majorRadius, 0.0, 0.0);
            majorAxis = majorAxis.RotateBy(Math.PI / 6, Vector3d.ZAxis);

            double radiusRatio = 0.25;

            pEllipse.Set(center, Vector3d.ZAxis, majorAxis, radiusRatio, 0, 2 * Math.PI);

            /**********************************************************************/
            /* Add an Attdef                                                      */
            /**********************************************************************/
            AttributeDefinition pAttDef = new AttributeDefinition();
            pAttDef.SetDatabaseDefaults(pDb);
            bBTR.AppendEntity(pAttDef);

            pAttDef.Prompt = "Enter ODT_ATT: ";
            pAttDef.Tag = "Oda_ATT";
            pAttDef.HorizontalMode = TextHorizontalMode.TextCenter;
            pAttDef.Height = 0.1;
            pAttDef.TextString = "Default";

            /**********************************************************************/
            /* Return the ObjectId of the BlockTableRecord                        */
            /**********************************************************************/
            return btrId;
        }

        //    /************************************************************************/
        //    /* Append an XData Pair to the specified ResBuf                         */
        //    /************************************************************************/
        //    OdResBufPtr appendXDataPair(OdResBufPtr pCurr, 
        //                                          int code)
        //    {
        //      pCurr.setNext(OdResBuf::newRb(code));
        //      return pCurr.next();
        //    }

        void addExtendedData(ObjectId id)
        {
            DBObject obj = id.GetObject(OpenMode.ForWrite);
            ResultBuffer rb = new ResultBuffer(
              new TypedValue(1001, "ODA"),
              new TypedValue(1000, "Extended Data for ODA app"),
              new TypedValue(1000, "Double"));

            obj.XData = rb;
        }

        // Adds an external reference called "XRefBlock") to the passed in database,
        // which references the file "xref.dwg").

        /************************************************************************/
        /* Add an XRef to the specified BlockTableRecord                        */
        /************************************************************************/
        void addXRef(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            using (BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite))
            {
                Database pDb = btrId.Database;

                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "XREF INSERT",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create a BlockTableRecord                                          */
                /**********************************************************************/
                ObjectId objIdXRef = pDb.AttachXref("OdWriteEx XRef.dwg", "XRefBlock");

                /**********************************************************************/
                /* Insert the Xref                                                    */
                /**********************************************************************/
                ObjectId xRefId = addInsert(bBTR, objIdXRef, 1.0, 1.0);
                /**********************************************************************/
                /* Open the insert                                                    */
                /**********************************************************************/
                BlockReference pXRefIns = (BlockReference)xRefId.GetObject(OpenMode.ForWrite);

                /**********************************************************************/
                /* Set the insertion point                                            */
                /**********************************************************************/
                pXRefIns.Position = point;

                /**********************************************************************/
                /* Move\Scale XREF to presentation rectangle                          */
                /**********************************************************************/
                //Extents3d extents = pXRefIns.GeometricExtents;
                /*Point3d maxPt = extents.MaxPoint;
                Point3d minPt = extents.MinPoint;
                if ((maxPt.X >= minPt.X) && (maxPt.Y >= minPt.Y) && (maxPt.Z >= minPt.Z))
                {
                  double dScale = Math.Min(w / (maxPt.X - minPt.X), h * (7.0 / 8.0) / (maxPt.Y - minPt.Y));
                  pXRefIns.ScaleFactors = new Scale3d(dScale, dScale, 1);
                  pXRefIns.Position = point - dScale * (minPt - point.GetAsVector()).GetAsVector();
                }*/
            }
        }

        /************************************************************************/
        /* Add a layout                                                         */
        /************************************************************************/
        void addLayout(Database pDb)
        {
            /********************************************************************/
            /* Create a new Layout                                              */
            /********************************************************************/
            LayoutManager lm = LayoutManager.Current;
            ObjectId layoutId = lm.CreateLayout("ODA Layout");
            using (Layout pLayout = (Layout)layoutId.GetObject(OpenMode.ForWrite))
            {
                /********************************************************************/
                /* Make it current, creating the overall PaperSpace viewport        */
                /********************************************************************/
                lm.CurrentLayout = String.Format("ODA Layout");

                /********************************************************************/
                /* Open the overall viewport for this layout                        */
                /********************************************************************/
                DWGdirect.DatabaseServices.Viewport pOverallViewport = (DWGdirect.DatabaseServices.Viewport)pDb.PaperSpaceVportId.GetObject(OpenMode.ForWrite);

                /********************************************************************/
                /* Get some useful parameters                                       */
                /********************************************************************/
                Point3d centerPoint = pOverallViewport.CenterPoint;

                /********************************************************************/
                /* Note:                                                            */
                /* If a viewport is an overall viewport,                            */
                /* the values returned by width() and height() must be divided by a */
                /* factor of 1.058, and the parameters of setWidth and setHeight()  */
                /* must be multiplied a like factor.                                */
                /********************************************************************/
                const double margin = 0.25;
                double overallWidth = pOverallViewport.Width / 1.058 - 2 * margin;
                double overallHeight = pOverallViewport.Height / 1.058 - 2 * margin;
                Vector3d vecTmp = new Vector3d(0.5 * overallWidth, 0.5 * overallHeight, 0.0);
                Point3d overallLLCorner = centerPoint - vecTmp;

                /********************************************************************/
                /* Open the PaperSpace BlockTableRecord for this layout             */
                /********************************************************************/
                using (BlockTableRecord pPS = (BlockTableRecord)pLayout.BlockTableRecordId.GetObject(OpenMode.ForWrite))
                {
                    /********************************************************************/
                    /* Create a new viewport, and append it to PaperSpace               */
                    /********************************************************************/
                    DWGdirect.DatabaseServices.Viewport pViewport = new DWGdirect.DatabaseServices.Viewport();
                    pViewport.SetDatabaseDefaults(pDb);
                    pPS.AppendEntity(pViewport);

                    /********************************************************************/
                    /* Set some parameters                                              */
                    /*                                                                  */
                    /* This viewport occupies the upper half of the overall viewport,   */
                    /* and displays all objects in model space                          */
                    /********************************************************************/

                    pViewport.Width = overallWidth;
                    pViewport.Height = overallHeight * 0.5;
                    Vector3d vecTmp2 = new Vector3d(0.0, 0.5 * pViewport.Height, 0.0);
                    pViewport.CenterPoint = centerPoint + vecTmp2;
                    pViewport.ViewCenter = pOverallViewport.ViewCenter;
                    //pViewport.zoomExtents();

                    /********************************************************************/
                    /* Create viewports for each of the entities that have been         */
                    /* pushBacked onto m_layoutEntities                                 */
                    /********************************************************************/

                    if (m_layoutEntities.Count > 0)
                    {
                        double widthFactor = 1.0 / m_layoutEntities.Count;
                        int i = 0;
                        foreach (ObjectId layId in m_layoutEntities)
                        {
                            i++;
                            Entity pEnt = (Entity)layId.GetObject(OpenMode.ForWrite);
                            Extents3d entityExtents = pEnt.GeometricExtents;

                            /**************************************************************/
                            /* Create a new viewport, and append it to PaperSpace         */
                            /**************************************************************/
                            DWGdirect.DatabaseServices.Viewport pViewportN = new DWGdirect.DatabaseServices.Viewport();
                            pViewportN.SetDatabaseDefaults(pDb);
                            pPS.AppendEntity(pViewportN);

                            /**************************************************************/
                            /* The viewports are tiled along the bottom of the overall    */
                            /* viewport                                                   */
                            /**************************************************************/
                            pViewportN.Width = overallWidth * widthFactor;
                            pViewportN.Height = overallHeight * 0.5;
                            Vector3d vecTmpN = new Vector3d((i + 0.5) * pViewportN.Width, 0.5 * pViewportN.Height, 0.0);
                            pViewportN.CenterPoint = overallLLCorner + vecTmpN;

                            /**************************************************************/
                            /* The target of the viewport is the midpoint of the entity   */
                            /* extents                                                    */
                            /**************************************************************/
                            Point3d minPt = entityExtents.MinPoint;
                            Point3d maxPt = entityExtents.MaxPoint;
                            pViewportN.ViewTarget = new Point3d((minPt.X + maxPt.X) / 2.0,
                                                                (minPt.Y + maxPt.Y) / 2.0,
                                                                (minPt.Z + maxPt.Z) / 2.0);

                            /**************************************************************/
                            /* The viewHeight is the larger of the height as defined by   */
                            /* the entityExtents, and the height required to display the  */
                            /* width of the entityExtents                                 */
                            /**************************************************************/
                            double viewHeight = Math.Abs(maxPt.Y - minPt.Y);
                            double viewWidth = Math.Abs(maxPt.X - minPt.X);
                            viewHeight = Math.Max(viewHeight, viewWidth * pViewportN.Height / pViewportN.Width);
                            pViewportN.ViewHeight = viewHeight * 1.05;
                        }
                    }
                }
                pDb.TileMode = true;
            }
        }

        /************************************************************************/
        /* Add entity boxes to specified BlockTableRecord                       */
        /************************************************************************/
        void createEntityBoxes(ObjectId btrId, ObjectId layerId)
        {
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Open the BlockTableRecord                                          */
                /**********************************************************************/
                BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);

                Point3d currentPoint;
                /**********************************************************************/
                /* Create a 2D polyline for each box                                  */
                /**********************************************************************/
                for (int j = 0; j < EntityBoxes.VER_BOXES; j++)
                {
                    for (int i = 0; i < EntityBoxes.HOR_BOXES; i++)
                    {
                        if (!m_EntityBoxes.isBox(j, i))
                            break;

                        double wCurBox = m_EntityBoxes.getWidth(j, i);
                        currentPoint = m_EntityBoxes.getBox(j, i);

                        Polyline2d pPline = new Polyline2d();
                        pPline.SetDatabaseDefaults(pDb);

                        bBTR.AppendEntity(pPline);

                        Vertex2d pVertex = new Vertex2d();
                        pVertex.SetDatabaseDefaults(pDb);
                        pPline.AppendVertex(pVertex);
                        Point3d pos = currentPoint;
                        pVertex.Position = pos;

                        pVertex = new Vertex2d();
                        pPline.AppendVertex(pVertex);
                        pos = pos + wCurBox * Vector3d.XAxis;
                        pVertex.Position = pos;

                        pVertex = new Vertex2d();
                        pPline.AppendVertex(pVertex);
                        pos = pos - m_EntityBoxes.getHeight() * Vector3d.YAxis;
                        pVertex.Position = pos;

                        pVertex = new Vertex2d();
                        pPline.AppendVertex(pVertex);
                        pos = pos - wCurBox * Vector3d.XAxis;
                        pVertex.Position = pos;

                        pPline.Closed = true;

                        pPline.ColorIndex = 5;
                        pPline.LayerId = layerId;
                    }
                }
                /**********************************************************************/
                /* 'Zoom' the box array by resizing the active tiled MS viewport      */
                /**********************************************************************/
                ViewportTableRecord vPortRec = (ViewportTableRecord)pDb.CurrentViewportTableRecordId.GetObject(OpenMode.ForWrite);

                Point3d center = m_EntityBoxes.getArrayCenter();
                vPortRec.CenterPoint = new Point2d(center.X, center.Y);

                Vector3d size = m_EntityBoxes.getArraySize();
                vPortRec.Height = 1.05 * Math.Abs(size.Y);
                vPortRec.Width = 1.05 * Math.Abs(size.X);
                vPortRec.CircleSides = 20000;
            }
        }

        /************************************************************************/
        /* Add a PaperSpace viewport to the specified database                  */
        /************************************************************************/
        void addPsViewport(Database pDb, ObjectId layerId)
        {
            /**********************************************************************/
            /* Enable PaperSpace                                                  */
            /*                                                                    */
            /* NOTE: This is required to cause DWGdirect to automatically create  */
            /* the overall viewport. If not called before opening PaperSpace      */
            /* BlockTableRecord,   the first viewport created IS the the overall  */
            /* viewport.                                                          */
            /**********************************************************************/
            pDb.TileMode = false;

            /**********************************************************************/
            /* Open PaperSpace                                                    */
            /**********************************************************************/
            BlockTable blTable = (BlockTable)pDb.BlockTableId.GetObject(OpenMode.ForRead);
            BlockTableRecord pPs = (BlockTableRecord)blTable[BlockTableRecord.PaperSpace].GetObject(OpenMode.ForWrite);

            /**********************************************************************/
            /* Disable PaperSpace                                                 */
            /**********************************************************************/
            pDb.TileMode = true;

            /**********************************************************************/
            /* Create the viewport                                                */
            /**********************************************************************/
            DWGdirect.DatabaseServices.Viewport pVp = new DWGdirect.DatabaseServices.Viewport();
            pVp.SetDatabaseDefaults(pDb);
            /**********************************************************************/
            /* Add it to PaperSpace                                               */
            /**********************************************************************/
            pPs.AppendEntity(pVp);

            /**********************************************************************/
            /* Set some parameters                                                */
            /**********************************************************************/
            pVp.CenterPoint = new Point3d(5.25, 4.0, 0);
            pVp.Width = 10.0;
            pVp.Height = 7.5;
            pVp.ViewTarget = Point3d.Origin;
            pVp.ViewDirection = Vector3d.ZAxis;
            pVp.ViewHeight = 8.0;
            pVp.LensLength = 50.0;
            pVp.ViewCenter = new Point2d(5.25, 4.0);
            pVp.SnapIncrement = new Vector2d(0.25, 0.25);
            pVp.GridIncrement = new Vector2d(0.25, 0.25);
            pVp.CircleSides = 20000;

            /**********************************************************************/
            /* Freeze a layer in this viewport                                    */
            /**********************************************************************/
            ObjectId[] layers = new ObjectId[] { layerId };
            pVp.FreezeLayersInViewport(layers.GetEnumerator());

            /**********************************************************************/
            /* Add a circle to this PaperSpace Layout                             */
            /**********************************************************************/
            Circle pCircle = new Circle();
            pCircle.SetDatabaseDefaults(pDb);
            pPs.AppendEntity(pCircle);
            pCircle.Radius = 1.0;
            pCircle.Center = new Point3d(1.0, 1.0, 0.0);
            pCircle.SetLayerId(layerId, false);

            /**********************************************************************/
            /* Disable PaperSpace                                                 */
            /**********************************************************************/
            pDb.TileMode = true;
        }

        /************************************************************************/
        /* Add a dimension style to the specified database                      */
        /************************************************************************/
        ObjectId addDimStyle(Database pDb, string dimStyleName)
        {
            /**********************************************************************/
            /* Create the DimStyle                                                */
            /**********************************************************************/
            DimStyleTableRecord pDimStyle = new DimStyleTableRecord();
            /**********************************************************************/
            /* Set the name                                                       */
            /**********************************************************************/
            pDimStyle.Name = dimStyleName;
            /**********************************************************************/
            /* Open the DimStyleTable                                             */
            /**********************************************************************/
            DimStyleTable pTable = (DimStyleTable)pDb.DimStyleTableId.GetObject(OpenMode.ForWrite);
            /**********************************************************************/
            /* Add the DimStyle                                                   */
            /**********************************************************************/
            ObjectId dimStyleId = pTable.Add(pDimStyle);
            /**********************************************************************/
            /* Set some properties                                                */
            /**********************************************************************/
            TextStyleTable textStyleTbl = (TextStyleTable)pDb.TextStyleTableId.GetObject(OpenMode.ForRead);
            pDimStyle.Dimtxsty = textStyleTbl["Standard"];
            pDimStyle.Dimsah = true;
            // TODO:
            //pDimStyle.Dimblk1 = "_OBLIQUE";
            //pDimStyle.Dimblk2 = "_DOT";
            return dimStyleId;
        }

        /************************************************************************/
        /* Add an Rotated Dimension to the specified BlockTableRecord               */
        /************************************************************************/
        void addRotatedDimension(ObjectId btrId,
                                  int boxRow,
                                  int boxCol,
                                  ObjectId layerId,
                                  ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d ulPoint = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the labels                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, ulPoint + m_textOffset, ulPoint + m_textOffset, "Rotated",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                addTextEnt(bBTR, ulPoint + m_textOffset + m_textLine, ulPoint + m_textOffset + m_textLine, "Dimension",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                Point3d llPoint = new Point3d(ulPoint.X, ulPoint.Y - h, ulPoint.Z);

                /**********************************************************************/
                /* Create a line to be dimensioned                                    */
                /**********************************************************************/
                Point3d line1Pt = new Point3d(llPoint.X + w * 1.0 / 8.0, llPoint.Y + h * 2.0 / 8.0, 0);
                Point3d line2Pt = new Point3d(line1Pt.X + 3.75, llPoint.Y + h * 7.0 / 8.0, 0);

                Line pLine = new Line(line1Pt, line2Pt);
                pLine.SetDatabaseDefaults(pDb);
                ObjectId lineId = bBTR.AppendEntity(pLine);

                /**********************************************************************/
                /* Create a rotated dimension and dimension the ends of the line      */
                /**********************************************************************/
                RotatedDimension pDimension = new RotatedDimension(0, pLine.StartPoint,
                  pLine.EndPoint, new Point3d(llPoint.X + w / 2.0, llPoint.Y + h * 1.0 / 8.0, 0), String.Format("RotatedDimension"), ObjectId.Null);
                pDimension.SetDatabaseDefaults(pDb);
                ObjectId dimensionId = bBTR.AppendEntity(pDimension);
                /*RotatedDimension pDimension = new RotatedDimension();
                pDimension.SetDatabaseDefaults(pDb);
                ObjectId dimensionId    = bBTR.AppendEntity(pDimension);
                pDimension.XLine1Point  = pLine.StartPoint;
                pDimension.XLine2Point  = pLine.EndPoint;
                pDimension.DimLinePoint = new Point3d(llPoint.X + w / 2.0, llPoint.Y + h * 1.0 / 8.0, 0);
                bool bTmp = pDimension.UsingDefaultTextPosition;
                pDimension.CreateExtensionDictionary();*/
            }
        }

        /************************************************************************/
        /* Add an Aligned Dimension to the specified BlockTableRecord           */
        /************************************************************************/
        void addAlignedDimension(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId, ObjectId dimStyleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);

            /**********************************************************************/
            /* Get the Upper-left corner of the box and its size                  */
            /**********************************************************************/
            Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
            double h = m_EntityBoxes.getHeight();
            double w = m_EntityBoxes.getWidth(boxRow, boxCol);

            /**********************************************************************/
            /* Add the labels                                                     */
            /**********************************************************************/
            addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Aligned", m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

            addTextEnt(bBTR, point + m_textOffset + m_textLine, point + m_textOffset + m_textLine,
              "Dimension", m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

            /**********************************************************************/
            /* Get the lower-left corner of the box                               */
            /**********************************************************************/
            point -= Vector3d.YAxis * h;

            /**********************************************************************/
            /* Create a line to be dimensioned                                    */
            /**********************************************************************/
            Point3d line1Pt = new Point3d(point.X + w * 0.5 / 8.0, point.Y + h * 1.5 / 8.0, 0);
            Point3d line2Pt = line1Pt + new Vector3d(1.5, 2.0, 0.0);

            Line pLine = new Line();
            using (Database pDb = bBTR.Database)
            {
                pLine.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pLine);
                pLine.StartPoint = line1Pt;
                pLine.EndPoint = line2Pt;

                /**********************************************************************/
                /* Create an aligned dimension and dimension the ends of the line     */
                /**********************************************************************/
                using (AlignedDimension pDimension = new AlignedDimension())
                {
                    pDimension.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(pDimension);

                    Point3d dimLinePt = new Point3d(point.X + w * 3.5 / 8.0, point.Y + h * 2.0 / 8.0, 0);

                    pDimension.DimensionStyle = dimStyleId;
                    pDimension.XLine1Point = pLine.StartPoint;
                    pDimension.XLine2Point = pLine.EndPoint;
                    pDimension.DimLinePoint = dimLinePt;
                    pDimension.UsingDefaultTextPosition = true;

                    // TODO
                    //pDimension.JogSymbolHeight(1.5);
                }
            }
        }

        /************************************************************************/
        /* Add a Radial Dimension to the specified BlockTableRecord             */
        /************************************************************************/
        void addRadialDimension(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                //  double w    = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the labels                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Radial",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                addTextEnt(bBTR, point + m_textOffset + m_textLine, point + m_textOffset + m_textLine,
                  "Dimension", m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create a circle to be dimensioned                                    */
                /**********************************************************************/
                Circle pCircle = new Circle();
                pCircle.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pCircle);
                pCircle.Center = point + new Vector3d(0.625, h * 3.0 / 8.0, 0);
                pCircle.Radius = 0.5;

                /**********************************************************************/
                /* Create a Radial Dimension                                         */
                /**********************************************************************/
                using (RadialDimension pDimension = new RadialDimension())
                {
                    pDimension.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(pDimension);

                    pDimension.Center = pCircle.Center;
                    Vector3d chordVector = new Vector3d(pCircle.Radius, 0.0, 0.0);
                    chordVector = chordVector.RotateBy(OdaToRadian(75.0), Vector3d.ZAxis);
                    pDimension.ChordPoint = pDimension.Center + chordVector;
                    pDimension.LeaderLength = 0.125;
                    pDimension.UsingDefaultTextPosition = true;
                }
            }
        }

        /************************************************************************/
        /* Add a Diametric Dimension to the specified BlockTableRecord             */
        /************************************************************************/
        void addDiametricDimension(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                //  double w    = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the labels                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Diametric",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                addTextEnt(bBTR, point + m_textOffset + m_textLine, point + m_textOffset + m_textLine,
                  "Dimension", m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create a circle to be dimensioned                                    */
                /**********************************************************************/
                Circle pCircle = new Circle();
                pCircle.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pCircle);
                pCircle.Center = point + new Vector3d(0.625, h * 3.0 / 8.0, 0);
                pCircle.Radius = 0.5;

                /**********************************************************************/
                /* Create a Diametric Dimension                                       */
                /**********************************************************************/
                using (DiametricDimension pDimension = new DiametricDimension())
                {
                    pDimension.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(pDimension);

                    Vector3d chordVector = new Vector3d(pCircle.Radius, 0.0, 0.0);
                    chordVector = chordVector.RotateBy(OdaToRadian(75.0), Vector3d.ZAxis);

                    pDimension.ChordPoint = pCircle.Center + chordVector;
                    pDimension.FarChordPoint = pCircle.Center - chordVector;
                    pDimension.LeaderLength = 0.125;
                    pDimension.UsingDefaultTextPosition = true;
                }
            }
        }

        /************************************************************************/
        /* Add a Shape to the specified BlockTableRecord                        */
        /************************************************************************/
        void addShape(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                //  double h    = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the labels                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Shape",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the center of the box                                          */
                /**********************************************************************/
                Point3d pCenter = m_EntityBoxes.getBoxCenter(boxRow, boxCol);

                /**********************************************************************/
                /* Create a Shape                                                     */
                /**********************************************************************/
                Shape pShape = new Shape();
                pShape.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pShape);
                double size = w * 3.0 / 8.0;
                pShape.Size = size;
                pShape.Position = pCenter + new Vector3d(0.0, -size, 0.0);
                pShape.Rotation = OdaToRadian(90.0);
                pShape.Name = "CIRC1";
            }
        }

        /************************************************************************/
        /* Add a 3D Face to the specified BlockTableRecord                      */
        /************************************************************************/
        void add3dFace(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "3DFACE",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create a 3D Face                                                   */
                /**********************************************************************/
                Face pFace = new Face();
                pFace.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pFace);

                pFace.SetVertexAt(0, point + new Vector3d(w * 1.0 / 8.0, h * 1.0 / 8.0, 0.0));
                pFace.SetVertexAt(1, point + new Vector3d(w * 7.0 / 8.0, h * 1.0 / 8.0, 0.0));
                pFace.SetVertexAt(2, point + new Vector3d(w * 7.0 / 8.0, h * 6.0 / 8.0, 0.0));
                pFace.SetVertexAt(3, point + new Vector3d(w * 1.0 / 8.0, h * 6.0 / 8.0, 0.0));
            }
        }

        /************************************************************************/
        /* Add a Solid to the specified BlockTableRecord                          */
        /************************************************************************/
        void addSolid(ObjectId btrId,
                                int boxRow,
                                int boxCol,
                                ObjectId layerId,
                                ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "SOLID",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;


                /**********************************************************************/
                /* Create a Solid                                                   */
                /**********************************************************************/
                Solid pSolid = new Solid();
                pSolid.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pSolid);

                pSolid.SetPointAt(0, point + new Vector3d(w * 1.0 / 8.0, h * 1.0 / 8.0, 0.0));
                pSolid.SetPointAt(1, point + new Vector3d(w * 7.0 / 8.0, h * 1.0 / 8.0, 0.0));
                pSolid.SetPointAt(2, point + new Vector3d(w * 1.0 / 8.0, h * 6.0 / 8.0, 0.0));
                pSolid.SetPointAt(3, point + new Vector3d(w * 7.0 / 8.0, h * 6.0 / 8.0, 0.0));
            }
        }

        /************************************************************************/
        /* Add an ACIS Solid to the specified BlockTableRecord                  */
        /************************************************************************/
        void addACIS(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                //  double h    = m_EntityBoxes.getHeight();
                //  double w    = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "3DSOLID",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                Matrix3d xfm = Matrix3d.Displacement(m_EntityBoxes.getBoxCenter(boxRow, boxCol).GetAsVector());

                /**********************************************************************/
                /* Read the solids in the .sat file                                   */
                /**********************************************************************/
                using (DBObjectCollection entities = Body.AcisIn("OdWriteEx.sat"))
                {
                    if (entities.Count > 0)
                    {
                        /********************************************************************/
                        /* Read the solids in the .sat file                                 */
                        /********************************************************************/
                        addTextEnt(bBTR, point + m_textOffset + m_textLine, point + m_textOffset + m_textLine, "from SAT file",
                        m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);
                        foreach (Entity ent in entities)
                        {
                            /******************************************************************/
                            /* Move the solid into the center of the box                      */
                            /******************************************************************/
                            ObjectId id = bBTR.AppendEntity(ent);
                            Entity p3dSolid = (Entity)id.GetObject(OpenMode.ForWrite);
                            p3dSolid.TransformBy(xfm);
                            /******************************************************************/
                            /* Each of these entities will later get its own viewport         */
                            /******************************************************************/
                            m_layoutEntities.Add(id);
                        }
                    }
                    else
                    {
                        /********************************************************************/
                        /* Create a simple solid                                            */
                        /********************************************************************/
                        using (Solid3d p3dSolid = new Solid3d())
                        {
                            p3dSolid.SetDatabaseDefaults(pDb);
                            ObjectId id = bBTR.AppendEntity(p3dSolid);

                            p3dSolid.CreateSphere(1.0);
                            p3dSolid.TransformBy(xfm);

                            /********************************************************************/
                            /* This entity will later get its own viewport                      */
                            /********************************************************************/
                            m_layoutEntities.Add(id);
                        }
                    }
                }
            }
        }
        /************************************************************************/
        /* Add a Box to the specified BlockTableRecord                          */
        /************************************************************************/
        void addBox(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Box",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                using (Solid3d p3dSolid = new Solid3d())
                {
                    p3dSolid.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(p3dSolid);

                    Matrix3d xfm = Matrix3d.Displacement(m_EntityBoxes.getBoxCenter(boxRow, boxCol).GetAsVector());

                    p3dSolid.CreateBox(w * 6.0 / 8.0, h * 6.0 / 8.0, w * 6.0 / 8.0);
                    p3dSolid.TransformBy(xfm);
                }
            }
        }

        /************************************************************************/
        /* Add a Frustum to the specified BlockTableRecord                      */
        /************************************************************************/
        void addFrustum(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Frustum",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);
                using (Solid3d p3dSolid = new Solid3d())
                {
                    p3dSolid.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(p3dSolid);

                    Matrix3d xfm = Matrix3d.Displacement(m_EntityBoxes.getBoxCenter(boxRow, boxCol).GetAsVector());

                    p3dSolid.CreateFrustum(w * 6.0 / 8.0, w * 3.0 / 8.0, h * 3.0 / 8.0, w * 1.0 / 8.0);
                    p3dSolid.TransformBy(xfm);
                }
            }
        }
        /************************************************************************/
        /* Add a Sphere to the specified BlockTableRecord                       */
        /************************************************************************/
        void addSphere(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Sphere",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                using (Solid3d p3dSolid = new Solid3d())
                {
                    p3dSolid.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(p3dSolid);

                    Matrix3d xfm = Matrix3d.Displacement(m_EntityBoxes.getBoxCenter(boxRow, boxCol).GetAsVector());

                    p3dSolid.CreateSphere(w * 3.0 / 8.0);
                    p3dSolid.TransformBy(xfm);
                }
            }
        }
        /************************************************************************/
        /* Add a Torus to the specified BlockTableRecord                       */
        /************************************************************************/
        void addTorus(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                //  double h    = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Torus",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                using (Solid3d p3dSolid = new Solid3d())
                {
                    p3dSolid.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(p3dSolid);

                    Matrix3d xfm = Matrix3d.Displacement(m_EntityBoxes.getBoxCenter(boxRow, boxCol).GetAsVector());

                    p3dSolid.CreateTorus(w * 2.0 / 8.0, w * 1.0 / 8.0);
                    p3dSolid.TransformBy(xfm);
                }
            }
        }
        /************************************************************************/
        /* Add a Wedge to the specified BlockTableRecord                       */
        /************************************************************************/
        void addWedge(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Wedge",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                using (Solid3d p3dSolid = new Solid3d())
                {
                    p3dSolid.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(p3dSolid);

                    Matrix3d xfm = Matrix3d.Displacement(m_EntityBoxes.getBoxCenter(boxRow, boxCol).GetAsVector());

                    p3dSolid.CreateWedge(w * 6.0 / 8.0, h * 6.0 / 8.0, w * 6.0 / 8.0);
                    p3dSolid.TransformBy(xfm);
                }
            }
        }

        /************************************************************************/
        /* Add a Region to the specified BlockTableRecord                       */
        /************************************************************************/
        void addRegion(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                //  double h    = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Region",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Create a Circle                                                    */
                /**********************************************************************/
                Circle pCircle = new Circle();
                pCircle.SetDatabaseDefaults(pDb);

                Point3d center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                pCircle.Center = center;
                pCircle.Radius = w * 3.0 / 8.0;


                /**********************************************************************/
                /* Add it to the array of curves                                      */
                /**********************************************************************/
                DBObjectCollection curveSegments = new DBObjectCollection();
                curveSegments.Add(pCircle);

                /**********************************************************************/
                /* Create the region                                                  */
                /**********************************************************************/
                using (DBObjectCollection regions = Region.CreateFromCurves(curveSegments))
                {
                    foreach (DBObject obj in regions)
                    {
                        /**********************************************************************/
                        /* Append it to the block table record                                */
                        /**********************************************************************/
                        bBTR.AppendEntity((Entity)obj);
                        // 3d solids should be disposed, because they lock modeler module
                        obj.Dispose();
                    }
                }
            }
        }

        /************************************************************************/
        /* Add an Extrusion to the specified BlockTableRecord                   */
        /************************************************************************/
        void addExtrusion(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                //  double h    = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Extrusion",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                using (Solid3d p3dSolid = new Solid3d())
                {
                    p3dSolid.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(p3dSolid);

                    /**********************************************************************/
                    /* Create a Circle                                                    */
                    /**********************************************************************/
                    Circle pCircle = new Circle();
                    pCircle.SetDatabaseDefaults(pDb);

                    Point3d center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                    pCircle.Center = center;
                    pCircle.Radius = w * 3.0 / 8.0;

                    /**********************************************************************/
                    /* Add it to the array of curves                                      */
                    /**********************************************************************/
                    DBObjectCollection entities = new DBObjectCollection();
                    entities.Add(pCircle);
                    /**********************************************************************/
                    /* Create a region                                                    */
                    /**********************************************************************/
                    using (DBObjectCollection regions = Region.CreateFromCurves(entities))
                    {
                        System.Collections.IEnumerator enumerator = regions.GetEnumerator();
                        /**********************************************************************/
                        /* Extrude the region                                                 */
                        /**********************************************************************/
                        if (enumerator.MoveNext())
                        {
                            p3dSolid.Extrude((Region)enumerator.Current, w * 6.0 / 8.0, 0);
                            // 3d solids should be disposed, because they lock modeler module
                            ((Region)enumerator.Current).Dispose();
                        }
                    }
                }
            }
        }
        /************************************************************************/
        /* Add an Solid of Revolution to the specified BlockTableRecord         */
        /************************************************************************/
        void addSolRev(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                //  double h    = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Solid of",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);
                addTextEnt(bBTR, point + m_textOffset + m_textLine, point + m_textOffset + m_textLine, "Revolution",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                using (Solid3d p3dSolid = new Solid3d())
                {
                    p3dSolid.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(p3dSolid);

                    /**********************************************************************/
                    /* Create a Circle                                                    */
                    /**********************************************************************/
                    Circle pCircle = new Circle();
                    pCircle.SetDatabaseDefaults(pDb);

                    Point3d center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                    pCircle.Center = center + new Vector3d(w * 2.5 / 8.0, 0.0, 0.0);
                    pCircle.Radius = w * 1.0 / 8.0;

                    /**********************************************************************/
                    /* Add it to the array of curves                                      */
                    /**********************************************************************/
                    DBObjectCollection entities = new DBObjectCollection();
                    entities.Add(pCircle);
                    /**********************************************************************/
                    /* Create a region                                                    */
                    /**********************************************************************/
                    using (DBObjectCollection regions = Region.CreateFromCurves(entities))
                    {
                        System.Collections.IEnumerator enumerator = regions.GetEnumerator();
                        /**********************************************************************/
                        /* revolve the region                                                 */
                        /**********************************************************************/
                        if (enumerator.MoveNext())
                        {
                            p3dSolid.Revolve((Region)enumerator.Current, center, new Vector3d(0.0, 1.0, 0.0), 2 * Math.PI);
                            // 3d solids should be disposed, because they lock modeler module
                            ((Region)enumerator.Current).Dispose();
                        }
                    }
                }
            }
        }

        void addHelix(ObjectId blockId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)blockId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Helix",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Create the Helix                                                   */
                /**********************************************************************/
                Helix pHelix = new Helix();
                pHelix.SetDatabaseDefaults(pDb);

                /**********************************************************************/
                /* Add the Helix to the database                                      */
                /**********************************************************************/
                bBTR.AppendEntity(pHelix);

                /**********************************************************************/
                /* Set the Helix's parameters                                         */
                /**********************************************************************/
                pHelix.Constrain = ConstrainType.Height;
                pHelix.Height = h;
                pHelix.SetAxisPoint(point + new Vector3d(w / 2.0, -h / 2.0, 0.0), true);
                pHelix.StartPoint = pHelix.GetAxisPoint() + new Vector3d(w / 6.0, 0.0, 0.0);
                pHelix.Twist = false;
                pHelix.TopRadius = w * 3.0 / 8.0;
                pHelix.Turns = 6;

                /**********************************************************************/
                /* Create the Helix geometry (confirm parameters are set)             */
                /**********************************************************************/
                pHelix.CreateHelix();
            }
        }

        /************************************************************************/
        /* Add an Image to the specified BlockTableRecord                       */
        /************************************************************************/
        void addImage(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Open the Image Dictionary                                          */
                /**********************************************************************/
                ObjectId imageDictId = RasterImageDef.CreateImageDictionary(pDb);
                using (DBDictionary pImageDict = (DBDictionary)imageDictId.GetObject(OpenMode.ForWrite))
                {
                    /**********************************************************************/
                    /* Create an ImageDef object                                          */
                    /**********************************************************************/
                    using (RasterImageDef pImageDef = new RasterImageDef())
                    {
                        ObjectId imageDefId = pImageDict.SetAt("OdWriteEx", pImageDef);

                        /**********************************************************************/
                        /* Set some parameters                                                */
                        /**********************************************************************/
                        pImageDef.SourceFileName = String.Format("OdWriteEx.jpg");
                        using (GIRasterImage img = new GIRasterImage(1024, 650, Units.Inch))
                        {
                            pImageDef.SetImage(img.UnmanagedObject, (IntPtr)0, true);

                            /**********************************************************************/
                            /* Create an Image object                                             */
                            /**********************************************************************/
                            using (RasterImage pImage = new RasterImage())
                            {
                                pImage.SetDatabaseDefaults(pDb);
                                bBTR.AppendEntity(pImage);

                                /**********************************************************************/
                                /* Set some parameters                                                */
                                /**********************************************************************/
                                pImage.ImageDefId = imageDefId;
                                pImage.Orientation = new CoordinateSystem3d(point, new Vector3d(w, 0, 0), new Vector3d(0.0, h, 0));
                                pImage.DisplayOptions = ImageDisplayOptions.Show | ImageDisplayOptions.ShowUnaligned;

                                /**********************************************************************/
                                /* Add the label                                                      */
                                /**********************************************************************/
                                point = m_EntityBoxes.getBox(boxRow, boxCol);
                                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "IMAGE",
                                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);
                            }
                        }
                    }
                }
            }
        }

        /************************************************************************/
        /* Add a Ray to the specified BlockTableRecord                          */
        /************************************************************************/
        void addRay(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                //  double w    = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "RAY",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create a Ray from the center of the box and passing through        */
                /* the lower-left corner of the box                                   */
                /**********************************************************************/
                Ray pRay = new Ray();
                pRay.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pRay);

                Point3d basePoint = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                Vector3d unitDir = (point - basePoint).GetNormal();

                pRay.BasePoint = basePoint;
                pRay.UnitDir = unitDir;
            }
        }

        /************************************************************************/
        /* Add an Xline to the specified BlockTableRecord                       */
        /************************************************************************/
        void addXline(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                //  double w    = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "XLINE",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create a Ray from the center of the box and passing through        */
                /* the lower-left corner of the box                                   */
                /**********************************************************************/
                Xline pXline = new Xline();
                pXline.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pXline);

                Point3d basePoint = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                Vector3d unitDir = (point - basePoint).GetNormal();

                pXline.BasePoint = basePoint;
                pXline.UnitDir = unitDir;
            }
        }

        /************************************************************************/
        /* Add Hatches to the specified BlockTableRecord                          */
        /************************************************************************/
        void addHatches(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                //  double h    = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);
                double delta = w / 12.0;

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "HATCHs",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Create a rectangular Hatch with a circular hole                    */
                /**********************************************************************/
                Hatch pHatch = new Hatch();
                pHatch.SetDatabaseDefaults(pDb);
                ObjectId whiteHatchId = bBTR.AppendEntity(pHatch);

                /**********************************************************************/
                /* Set some properties                                                */
                /**********************************************************************/
                pHatch.Associative = false;
                pHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                pHatch.HatchStyle = HatchStyle.Normal;

                /**********************************************************************/
                /* Define the outer loop with an OdGePolyline2d                       */
                /**********************************************************************/
                HatchLoop loop = new HatchLoop(HatchLoopTypes.External | HatchLoopTypes.Polyline);
                loop.Polyline.Add(new BulgeVertex(new Point2d(point.X + delta, point.Y - delta), 0));
                loop.Polyline.Add(new BulgeVertex(new Point2d(point.X + delta * 5, point.Y - delta), 0));
                loop.Polyline.Add(new BulgeVertex(new Point2d(point.X + delta * 5, point.Y - delta * 5), 0));
                loop.Polyline.Add(new BulgeVertex(new Point2d(point.X + delta, point.Y - delta * 5), 0));
                pHatch.AppendLoop(loop);

                /**********************************************************************/
                /* Define an inner loop with an array of edges                        */
                /**********************************************************************/
                Point2d cenPt = new Point2d(point.X + delta * 3, point.Y - delta * 3);
                using (CircularArc2d cirArc = new CircularArc2d())
                {
                    cirArc.Center = cenPt;
                    cirArc.Radius = delta;
                    cirArc.SetAngles(0.0, 2 * Math.PI);

                    loop = new HatchLoop(HatchLoopTypes.Default);
                    loop.Curves.Add(cirArc); // TODO: Cause of crash
                }
                pHatch.AppendLoop(loop);

                /**********************************************************************/
                /* Create a circular Hatch                                            */
                /**********************************************************************/
                pHatch = new Hatch();
                pHatch.SetDatabaseDefaults(pDb);
                ObjectId redHatchId = bBTR.AppendEntity(pHatch);

                /**********************************************************************/
                /* Set some properties                                                */
                /**********************************************************************/
                pHatch.Associative = false;
                pHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                pHatch.HatchStyle = HatchStyle.Normal;
                Color col = Color.FromRgb(255, 0, 0);
                pHatch.Color = col;

                /**********************************************************************/
                /* Define an outer loop with an array of edges                        */
                /**********************************************************************/
                using (CircularArc2d cirArc = new CircularArc2d())
                {
                    cirArc.Center = cenPt - new Vector2d(delta, 0.0);
                    cirArc.Radius = delta;
                    cirArc.SetAngles(0.0, 2 * Math.PI);

                    loop = new HatchLoop(HatchLoopTypes.Default);
                    loop.Curves.Add(cirArc);
                }
                pHatch.AppendLoop(loop);

                /**********************************************************************/
                /* Create a circular Hatch                                            */
                /**********************************************************************/
                pHatch = new Hatch();
                pHatch.SetDatabaseDefaults(pDb);
                ObjectId greenHatchId = bBTR.AppendEntity(pHatch);

                pHatch.Associative = false;
                pHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                pHatch.HatchStyle = HatchStyle.Normal;
                col = Color.FromRgb(0, 255, 0);
                pHatch.Color = col;

                /**********************************************************************/
                /* Define an outer loop with an array of edges                        */
                /**********************************************************************/
                using (CircularArc2d cirArc = new CircularArc2d())
                {
                    cirArc.Center = cenPt - new Vector2d(0.0, delta);
                    cirArc.Radius = delta;
                    cirArc.SetAngles(0.0, 2 * Math.PI);

                    loop = new HatchLoop(HatchLoopTypes.Default);
                    loop.Curves.Add(cirArc);
                }
                pHatch.AppendLoop(loop);

                /**********************************************************************/
                /* Use the SortentsTable to manipulate draw order                     */
                /*                                                                    */
                /* The draw order now is white, red, green                            */
                /**********************************************************************/
                DrawOrderTable pSET = (DrawOrderTable)bBTR.DrawOrderTableId.GetObject(OpenMode.ForWrite);

                /**********************************************************************/
                /* Move the green hatch below the red hatch                           */
                /* The draw order now is white, green, red                            */
                /**********************************************************************/
                ObjectIdCollection id = new ObjectIdCollection();
                id.Add(greenHatchId);
                pSET.MoveBelow(id, redHatchId);

                /**********************************************************************/
                /* Create an associative user-defined hatch                           */
                /**********************************************************************/
                pHatch = new Hatch();
                pHatch.SetDatabaseDefaults(pDb);
                ObjectId hatchId = bBTR.AppendEntity(pHatch);

                /**********************************************************************/
                /* Set some properties                                                */
                /**********************************************************************/
                pHatch.Associative = true;
                pHatch.SetDatabaseDefaults(pDb); // make hatch aware of DB for the next call
                pHatch.SetHatchPattern(HatchPatternType.UserDefined, "_USER");
                pHatch.PatternSpace = 0.12;
                pHatch.PatternAngle = OdaToRadian(30.0);
                pHatch.PatternDouble = true;
                pHatch.HatchStyle = HatchStyle.Normal;

                /**********************************************************************/
                /* Define the loops                                                */
                /**********************************************************************/
                ObjectIdCollection loopIds = new ObjectIdCollection();
                Ellipse pEllipse = new Ellipse();
                pEllipse.SetDatabaseDefaults(pDb);
                loopIds.Add(bBTR.AppendEntity(pEllipse));

                Point3d centerPt = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                centerPt += new Vector3d(delta, delta * 1.5, 0);
                pEllipse.Set(centerPt, Vector3d.ZAxis, new Vector3d(delta, 0.0, 0.0), 0.5, 0, 2 * Math.PI);

                /**********************************************************************/
                /* Append the loops to the hatch                                      */
                /**********************************************************************/
                pHatch.AppendLoop(HatchLoopTypes.Default, loopIds);

                try
                {
                    /********************************************************************/
                    /* Create an associative predefined hatch                           */
                    /********************************************************************/
                    pHatch = new Hatch();
                    pHatch.SetDatabaseDefaults(pDb);
                    hatchId = bBTR.AppendEntity(pHatch);

                    /********************************************************************/
                    /* Set some properties                                              */
                    /********************************************************************/
                    point = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                    // Set the hatch properties.
                    pHatch.Associative = true;
                    pHatch.SetDatabaseDefaults(pDb);// make hatch aware of DB for the next call
                    pHatch.SetHatchPattern(HatchPatternType.PreDefined, "ANGLE");
                    pHatch.PatternScale = 0.5;
                    pHatch.PatternAngle = 0.5; // near 30 degrees
                    pHatch.HatchStyle = HatchStyle.Normal;


                    /********************************************************************/
                    /* Define the loops                                                 */
                    /********************************************************************/
                    loopIds.Clear();
                    Circle pCircle = new Circle();
                    pCircle.SetDatabaseDefaults(pDb);
                    loopIds.Add(bBTR.AppendEntity(pCircle));
                    centerPt -= new Vector3d(delta * 2.0, delta * 2.5, 0);
                    pCircle.Center = centerPt;
                    pCircle.Radius = delta * 1.5;

                    /********************************************************************/
                    /* Append the loops to the hatch                                    */
                    /********************************************************************/
                    pHatch.AppendLoop(HatchLoopTypes.Default, loopIds);
                }
                catch (DWGdirect.Runtime.Exception e)
                {
                    Console.WriteLine("\n\nException occurred: {0}\n", e.Message);
                    Console.WriteLine("\nHatch with predefined pattern \"ANGLE\" was not added.\n");
                    Console.WriteLine("\nMake sure PAT file with pattern definition is available to DWGdirect.");
                    Console.WriteLine("\n\nPress ENTER to continue...");
                    Console.ReadLine();
                }
            }
        }

        /************************************************************************/
        /* Add an Arc Dimension to the specified BlockTableRecord               */
        /************************************************************************/
        void addArcDimension(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the labels                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Arc",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                addTextEnt(bBTR, point + m_textOffset + m_textLine, point + m_textOffset + m_textLine,
                  "Dimension", m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create an arc to be dimensioned                                    */
                /**********************************************************************/
                Arc pArc = new Arc();
                pArc.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pArc);
                Point3d center = point + new Vector3d(w * 1.0 / 8.0, h * 1.0 / 8.0, 0.0);
                pArc.Center = center;
                pArc.StartAngle = OdaToRadian(0.0);
                pArc.EndAngle = OdaToRadian(90.0);
                pArc.Radius = 4.0 / Math.PI;


                /**********************************************************************/
                /* Create an ArcDimension                                             */
                /**********************************************************************/
                ArcDimension pDimension = new ArcDimension(pArc.Center, pArc.StartPoint, pArc.EndPoint, pArc.Center + new Vector3d(pArc.Radius + 0.45, 0.0, 0.0), "", ObjectId.Null);
                pDimension.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pDimension);

                /**********************************************************************/
                /* Use the default dim variables                                      */
                /**********************************************************************/
                pDimension.SetDatabaseDefaults(pDb);

                /**********************************************************************/
                /* Set some parameters                                                */
                /**********************************************************************/
                pDimension.ArcSymbolType = 1;
            }
        }

        /************************************************************************/
        /* Add a 3 Point Angular Dimension to the specified BlockTableRecord    */
        /************************************************************************/
        void add3PointAngularDimension(ObjectId btrId,
                                                 int boxRow,
                                                 int boxCol,
                                                 ObjectId layerId,
                                                 ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the labels                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "3 Point Angular",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                addTextEnt(bBTR, point + m_textOffset + m_textLine, point + m_textOffset + m_textLine, "Dimension",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create an arc to be dimensioned                                    */
                /**********************************************************************/
                Arc pArc = new Arc();
                pArc.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pArc);
                Point3d center = point + new Vector3d(w * 1.0 / 8.0, h * 1.0 / 8.0, 0.0);
                pArc.Center = center;
                pArc.StartAngle = OdaToRadian(0.0);
                pArc.EndAngle = OdaToRadian(90.0);
                pArc.Radius = w * 3.0 / 8.0;

                /**********************************************************************/
                /* Create 3 point angular dimension                                   */
                /**********************************************************************/
                using (Point3AngularDimension pDimension = new Point3AngularDimension())
                {
                    pDimension.SetDatabaseDefaults(pDb);
                    bBTR.AppendEntity(pDimension);

                    /**********************************************************************/
                    /* Use the default dim variables                                      */
                    /**********************************************************************/
                    pDimension.SetDatabaseDefaults(pDb);

                    /**********************************************************************/
                    /* Set some parameters                                                */
                    /**********************************************************************/
                    pDimension.CenterPoint = pArc.Center;
                    pDimension.ArcPoint = pArc.Center + new Vector3d(pArc.Radius + 0.45, 0.0, 0.0);

                    pDimension.XLine1Point = pArc.StartPoint;
                    pDimension.XLine2Point = pArc.EndPoint;
                }
            }
        }

        /************************************************************************/
        /* Add a 2 Line Angular Dimension to the specified BlockTableRecord     */
        /************************************************************************/
        void add2LineAngularDimension(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the labels                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "2 Line Angular",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                addTextEnt(bBTR, point + m_textOffset + m_textLine, point + m_textOffset + m_textLine,
                  "Dimension", m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create the lines to be dimensioned                                 */
                /**********************************************************************/
                Point3d center = point + new Vector3d(w * 1.0 / 8.0, h * 1.0 / 8.0, 0.0);
                Vector3d v1 = new Vector3d(w * 1.0 / 8.0, 0.0, 0.0);
                Vector3d v2 = new Vector3d(w * 4.0 / 8.0, 0.0, 0.0);
                Vector3d v3 = v2 + new Vector3d(0.45, 0.0, 0.0);

                Line pLine1 = new Line();
                pLine1.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pLine1);
                pLine1.StartPoint = center + v1;
                pLine1.EndPoint = center + v2;

                double rot = OdaToRadian(75.0);
                v1 = v1.RotateBy(rot, Vector3d.ZAxis);
                v2 = v2.RotateBy(rot, Vector3d.ZAxis);

                Line pLine2 = new Line();
                pLine2.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pLine2);
                pLine2.StartPoint = center + v1;
                pLine2.EndPoint = center + v2;

                /**********************************************************************/
                /* Create 2 Line Angular Dimensionn                                   */
                /**********************************************************************/
                using (LineAngularDimension2 pDimension = new LineAngularDimension2())
                {
                    bBTR.AppendEntity(pDimension);

                    /**********************************************************************/
                    /* Use the default dim variables                                      */
                    /**********************************************************************/
                    pDimension.SetDatabaseDefaults(pDb);

                    /**********************************************************************/
                    /* Set some parameters                                                */
                    /**********************************************************************/

                    v3 = v3.RotateBy(rot / 2.0, Vector3d.ZAxis);
                    pDimension.ArcPoint = center + v3;

                    pDimension.XLine1Start = pLine1.StartPoint;
                    pDimension.XLine1End = pLine1.EndPoint;

                    //  pDimension.setArcPoint(endPoint + 0.45*(endPoint - startPoint).normalize());

                    pDimension.XLine2Start = pLine2.StartPoint;
                    pDimension.XLine2End = pLine2.EndPoint;
                }
            }
        }

        /************************************************************************/
        /* Add a RadialDimensionLarge to the specified BlockTableRecord         */
        /************************************************************************/
        void addRadialDimensionLarge(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the labels                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Radia",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                addTextEnt(bBTR, point + m_textOffset + m_textLine, point + m_textOffset + m_textLine,
                  "Dim Large", m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create an arc to be dimensioned                                    */
                /**********************************************************************/
                Arc pArc = new Arc();
                pArc.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pArc);

                Point3d center = point + new Vector3d(w * 1.0 / 8.0, h * 1.0 / 8.0, 0.0);
                pArc.Radius = 2.0;

                pArc.Center = center;
                pArc.StartAngle = OdaToRadian(30.0);
                pArc.EndAngle = OdaToRadian(90.0);

                /**********************************************************************/
                /* Create RadialDimensionLarge                                        */
                /**********************************************************************/
                using (RadialDimensionLarge pDimension = new RadialDimensionLarge())
                {
                    bBTR.AppendEntity(pDimension);

                    /**********************************************************************/
                    /* Use the default dim variables                                      */
                    /**********************************************************************/
                    pDimension.SetDatabaseDefaults(pDb);

                    /**********************************************************************/
                    /* Set some parameters                                                */
                    /**********************************************************************/
                    Point3d centerPoint, chordPoint, overrideCenter, jogPoint, textPosition;

                    // The centerPoint of the dimension is the center of the arc
                    centerPoint = pArc.Center;

                    // The chordPoint of the dimension is the midpoint of the arc
                    chordPoint = centerPoint + new Vector3d(pArc.Radius, 0.0, 0.0).RotateBy(0.5 * (pArc.StartAngle + pArc.EndAngle), Vector3d.ZAxis);

                    // The overrideCenter is just to the right of the actual center
                    overrideCenter = centerPoint + new Vector3d(w * 3.0 / 8.0, 0.0, 0.0);

                    // The jogPoint is halfway between the overrideCenter and the chordCoint
                    jogPoint = overrideCenter + 0.5 * (chordPoint - overrideCenter);

                    // The textPosition is along the vector between the centerPoint and the chordPoint.
                    textPosition = centerPoint + 0.7 * (chordPoint - centerPoint);

                    double jogAngle = OdaToRadian(45.0);

                    pDimension.Center = centerPoint;
                    pDimension.ChordPoint = chordPoint;
                    pDimension.OverrideCenter = overrideCenter;
                    pDimension.JogPoint = jogPoint;
                    pDimension.TextPosition = textPosition;
                    pDimension.JogAngle = jogAngle;
                }
            }
        }

        /************************************************************************/
        /* Add Ordinate Dimensions to the specified BlockTableRecord            */
        /************************************************************************/
        void addOrdinateDimensions(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {

                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the labels                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "Ordinate",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                addTextEnt(bBTR, point + m_textOffset + m_textLine, point + m_textOffset + m_textLine,
                  "Dimension", m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                double dx = w / 8.0;
                double dy = h / 8.0;
                /**********************************************************************/
                /* Create a line to be dimensioned                                    */
                /**********************************************************************/
                Line pLine = new Line();
                pLine.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pLine);

                Point3d point1 = point + new Vector3d(dx, dy, 0.0);
                Point3d point2 = point1 + new Vector3d(0.0, 1.5, 0);
                pLine.StartPoint = point1;
                pLine.EndPoint = point2;

                /**********************************************************************/
                /* Create the base ordinate dimension                                 */
                /**********************************************************************/
                Point3d endPoint, startPoint, leaderEndPoint;
                using (OrdinateDimension pDimension = new OrdinateDimension())
                {
                    bBTR.AppendEntity(pDimension);

                    /**********************************************************************/
                    /* Use the default dim variables                                      */
                    /**********************************************************************/
                    pDimension.SetDatabaseDefaults(pDb);

                    /**********************************************************************/
                    /* Set some parameters                                                */
                    /**********************************************************************/

                    startPoint = pLine.StartPoint;
                    endPoint = pLine.EndPoint;

                    leaderEndPoint = startPoint + new Vector3d(3.0 * dx, 0, 0.0);

                    pDimension.Origin = startPoint;
                    pDimension.DefiningPoint = startPoint;
                    pDimension.LeaderEndPoint = leaderEndPoint;
                    pDimension.UsingXAxis = false;
                }

                /**********************************************************************/
                /* Create an ordinate dimension                                       */
                /**********************************************************************/
                using (OrdinateDimension pDimension = new OrdinateDimension())
                {
                    bBTR.AppendEntity(pDimension);

                    /**********************************************************************/
                    /* Use the default dim variables                                      */
                    /**********************************************************************/
                    pDimension.SetDatabaseDefaults(pDb);

                    /**********************************************************************/
                    /* Set some parameters                                                */
                    /**********************************************************************/
                    leaderEndPoint = endPoint + new Vector3d(3.0 * dx, -dy, 0.0);

                    pDimension.Origin = startPoint;
                    pDimension.DefiningPoint = endPoint;
                    pDimension.LeaderEndPoint = leaderEndPoint;
                    pDimension.UsingXAxis = false;
                }
            }
        }

        /************************************************************************/
        /* Add a Spline to the specified BlockTableRecord                       */
        /************************************************************************/
        void addSpline(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "SPLINE",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create the fit points                                              */
                /**********************************************************************/

                double dx = w / 8.0;
                double dy = h / 8.0;

                Point3dCollection fitPoints = new Point3dCollection();
                fitPoints.Add(point + new Vector3d(1.0 * dx, 1.0 * dy, 0.0));
                fitPoints.Add(point + new Vector3d(3.0 * dx, 6.0 * dy, 0.0));
                fitPoints.Add(point + new Vector3d(4.0 * dx, 2.0 * dy, 0.0));
                fitPoints.Add(point + new Vector3d(7.0 * dx, 7.0 * dy, 0.0));

                /**********************************************************************/
                /* Create Spline                                                      */
                /**********************************************************************/
                Spline pSpline = new Spline(fitPoints, new Vector3d(0, 0, 0), new Vector3d(1.0, 0.0, 0.0), 3, 0.0);
                pSpline.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pSpline);
            }
        }

        /************************************************************************/
        /* Add some Traces to the specified BlockTableRecord                    */
        /************************************************************************/
        void addTraces(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "TRACEs",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create a Trace                                                     */
                /**********************************************************************/
                Trace pTrace = new Trace();
                pTrace.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pTrace);

                double dx = w / 8.0;
                double dy = h / 8.0;
                pTrace.SetPointAt(0, point + new Vector3d(1.0 * dx, 2.0 * dx, 0.0));
                pTrace.SetPointAt(1, point + new Vector3d(1.0 * dx, 1.0 * dx, 0.0));
                pTrace.SetPointAt(2, point + new Vector3d(6.0 * dx, 2.0 * dx, 0.0));
                pTrace.SetPointAt(3, point + new Vector3d(7.0 * dx, 1.0 * dx, 0.0));

                /**********************************************************************/
                /* Create a Trace                                                     */
                /**********************************************************************/
                pTrace = new Trace();
                pTrace.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pTrace);

                pTrace.SetPointAt(0, point + new Vector3d(6.0 * dx, 2.0 * dx, 0.0));
                pTrace.SetPointAt(1, point + new Vector3d(7.0 * dx, 1.0 * dx, 0.0));
                pTrace.SetPointAt(2, point + new Vector3d(6.0 * dx, 7.0 * dy, 0.0));
                pTrace.SetPointAt(3, point + new Vector3d(7.0 * dx, 7.0 * dy, 0.0));
            }
        }
        /************************************************************************/
        /* Add an Mline to the specified BlockTableRecord                       */
        /************************************************************************/
        void addMLine(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = btrId.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the labels                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "MLINE",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of MLine                                 */
                /**********************************************************************/
                point += new Vector3d(w / 10.0, -h / 2, 0);

                /**********************************************************************/
                /* Create an MLine and add it to the database                         */
                /**********************************************************************/
                Mline pMLine = new Mline();
                pMLine.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pMLine);

                /**********************************************************************/
                /* Open the MLineStyle dictionary, and set the style                  */
                /**********************************************************************/
                DBDictionary pMLDic = (DBDictionary)pDb.MLStyleDictionaryId.GetObject(OpenMode.ForRead);
                pMLine.Style = pMLDic.GetAt("OdaStandard");

                /**********************************************************************/
                /* Add some segments                                                  */
                /**********************************************************************/
                point -= new Vector3d(0, h / 2.2, 0);
                pMLine.AppendSegment(point);

                point += new Vector3d(0, h / 3.0, 0);
                pMLine.AppendSegment(point);

                point += new Vector3d(w / 4.0, h / 5.0, 0);
                pMLine.AppendSegment(point);

                point += new Vector3d(w / 4.0, 0, 0);
                pMLine.AppendSegment(point);

                point += new Vector3d(0, h / 3.0, 0);
                pMLine.AppendSegment(point);

                point += new Vector3d(w / 3, 0, 0);
                pMLine.AppendSegment(point);

                point -= new Vector3d(0, h / 2, 0);
                pMLine.AppendSegment(point);

                point -= new Vector3d(w / 4, h / 3, 0);
                pMLine.AppendSegment(point);
            }
        }

        /************************************************************************/
        /* Add a Polyline to the specified BlockTableRecord                     */
        /************************************************************************/
        void addPolyline(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "LWPOLYLINE",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create a polyline                                                  */
                /**********************************************************************/
                DWGdirect.DatabaseServices.Polyline pPolyline = new DWGdirect.DatabaseServices.Polyline();
                pPolyline.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pPolyline);

                /**********************************************************************/
                /* Create the vertices                                                */
                /**********************************************************************/

                double dx = w / 8.0;
                double dy = h / 8.0;

                Point2d point2d = new Point2d(point.X + 1.5 * dx, point.Y + 3.0 * dy);

                pPolyline.AddVertexAt(0, point2d, 0, -1, -1);

                point2d -= new Vector2d(0, 1) * 0.5 * dy;
                pPolyline.AddVertexAt(1, point2d, 1.0, -1, -1);

                point2d += new Vector2d(1, 0) * 5.0 * dx;
                pPolyline.AddVertexAt(2, point2d, 0, -1, -1);

                point2d += new Vector2d(0, 1) * 4.0 * dy;
                pPolyline.AddVertexAt(3, point2d, 0, -1, -1);

                point2d -= new Vector2d(1, 0) * 1.0 * dx;
                pPolyline.AddVertexAt(4, point2d, 0, -1, -1);

                point2d -= new Vector2d(0, 1) * 4.0 * dy;
                pPolyline.AddVertexAt(5, point2d, -1, -1, -1);

                point2d -= new Vector2d(1, 0) * 3.0 * dx;
                pPolyline.AddVertexAt(6, point2d, 0, -1, -1);

                point2d += new Vector2d(0, 1) * 0.5 * dy;
                pPolyline.AddVertexAt(7, point2d, 0, -1, -1);

                pPolyline.Closed = true;
            }
        }

        /************************************************************************/
        /* Add a Wipeout to to the specified BlockTableRecord                   */
        /************************************************************************/
        void addWipeout(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord bBTR = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = bBTR.Database)
            {
                /**********************************************************************/
                /* Get the lower-left corner and center of the box                    */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                Point3d center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                     */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset, "WIPEOUT",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Get the lower-left corner of the box                               */
                /**********************************************************************/
                point -= Vector3d.YAxis * h;

                /**********************************************************************/
                /* Create a hatch object to be wiped out                              */
                /**********************************************************************/
                Hatch pHatch = new Hatch();
                pHatch.SetDatabaseDefaults(pDb);
                ObjectId hatchId = bBTR.AppendEntity(pHatch);

                /**********************************************************************/
                /* Create a hatch object to be wiped out                              */
                /**********************************************************************/
                pHatch.Associative = true;
                pHatch.SetHatchPattern(HatchPatternType.UserDefined, "_USER");
                pHatch.PatternSpace = 0.125;
                pHatch.PatternAngle = 0.5; // near 30 degrees
                pHatch.PatternDouble = true; // Cross hatch
                pHatch.HatchStyle = HatchStyle.Normal;

                /**********************************************************************/
                /* Create an outer loop for the hatch                                 */
                /**********************************************************************/
                Circle pCircle = new Circle();
                pCircle.SetDatabaseDefaults(pDb);
                ObjectIdCollection loopIds = new ObjectIdCollection();
                loopIds.Add(bBTR.AppendEntity(pCircle));
                pCircle.Center = center;
                pCircle.Radius = Math.Min(w, h) * 0.4;
                pHatch.AppendLoop(HatchLoopTypes.Default, loopIds);

                /**********************************************************************/
                /* Create the wipeout                                                  */
                /**********************************************************************/
                Wipeout pWipeout = new Wipeout();
                pWipeout.SetDatabaseDefaults(pDb);
                bBTR.AppendEntity(pWipeout);

                Point2d center2d = new Point2d(center.X, center.Y);
                Point2dCollection boundary = new Point2dCollection();
                boundary.Add(center2d + new Vector2d(-w * 0.4, -h * 0.4));
                boundary.Add(center2d + new Vector2d(w * 0.4, -h * 0.4));
                boundary.Add(center2d + new Vector2d(0.0, h * 0.4));
                boundary.Add(center2d + new Vector2d(-w * 0.4, -h * 0.4));

                pWipeout.SetClipBoundary(ClipBoundaryType.Poly, boundary);

                pWipeout.DisplayOptions = ImageDisplayOptions.Show
                                        | ImageDisplayOptions.Clip
                                        | ImageDisplayOptions.Transparent;
            }
        }

        /************************************************************************/
        /* Add a Table to the specified BlockTableRecord                        */
        /************************************************************************/
        void addTable(ObjectId btrId, ObjectId addedBlockId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            BlockTableRecord pRecord = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);
            using (Database pDb = pRecord.Database)
            {
                /**********************************************************************/
                /* Get the lower-left corner and center of the box                    */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                Point3d center = m_EntityBoxes.getBoxCenter(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);


                /**********************************************************************/
                /* Create the Table                                                  */
                /**********************************************************************/
                Table pAcadTable = new Table();
                ObjectId tableId = pRecord.AppendEntity(pAcadTable);
                /**********************************************************************/
                /* This entity will later get its own viewport                        */
                /**********************************************************************/
                m_layoutEntities.Add(tableId);

                /**********************************************************************/
                /* Set the parameters                                                 */
                /**********************************************************************/
                pAcadTable.SetDatabaseDefaults(pRecord.Database);
                pAcadTable.NumColumns = 3;
                pAcadTable.NumRows = 4;

                pAcadTable.GenerateLayout();
                pAcadTable.SetColumnWidth(w / pAcadTable.NumColumns);
                pAcadTable.SetRowHeight(h / pAcadTable.NumRows);

                pAcadTable.Position = point;
                pAcadTable.SetTextStyle(styleId, (int)(RowType.DataRow | RowType.HeaderRow | RowType.TitleRow));

                pAcadTable.SetTextHeight(0.500 * pAcadTable.RowHeight(0), (int)RowType.TitleRow);
                pAcadTable.SetTextHeight(0.300 * pAcadTable.RowHeight(1), (int)RowType.HeaderRow);
                pAcadTable.SetTextHeight(0.250 * pAcadTable.RowHeight(2), (int)RowType.DataRow);

                /**********************************************************************/
                /* Set the alignments                                                 */
                /**********************************************************************/
                for (int row = 1; row < (int)pAcadTable.NumRows; row++)
                {
                    for (int col = 0; col < (int)pAcadTable.NumColumns; col++)
                    {
                        pAcadTable.SetAlignment(row, col, CellAlignment.MiddleCenter);
                    }
                }

                /**********************************************************************/
                /* Define the title row                                               */
                /**********************************************************************/
                CellRange cRange = CellRange.Create(pAcadTable, 0, 0, 0, pAcadTable.NumColumns - 1);
                pAcadTable.MergeCells(cRange);
                pAcadTable.SetTextString(0, 0, "Title of TABLE");

                /**********************************************************************/
                /* Define the header row                                              */
                /**********************************************************************/
                pAcadTable.SetTextString(1, 0, "Header0");
                pAcadTable.SetTextString(1, 1, "Header1");
                pAcadTable.SetTextString(1, 2, "Header2");

                /**********************************************************************/
                /* Define the first data row                                          */
                /**********************************************************************/
                pAcadTable.SetTextString(2, 0, "Data0");
                pAcadTable.SetTextString(2, 1, "Data1");
                pAcadTable.SetTextString(2, 2, "Data2");

                /**********************************************************************/
                /* Define the second data row                                         */
                /**********************************************************************/
                pAcadTable.SetCellType(3, 0, TableCellType.BlockCell);
                pAcadTable.SetBlockTableRecordId(3, 0, addedBlockId, false);
                pAcadTable.SetBlockScale(3, 0, 1.0);
                pAcadTable.SetAutoScale(3, 0, true);
                pAcadTable.SetBlockRotation(3, 0, 0.0);

                pAcadTable.SetTextString(3, 1, "<-Block Cell.");

                pAcadTable.SetCellType(3, 2, TableCellType.BlockCell);
                pAcadTable.SetBlockTableRecordId(3, 2, addedBlockId, false);
                pAcadTable.SetAutoScale(3, 2, true);
                pAcadTable.SetBlockRotation(3, 2, OdaToRadian(30.0));

                pAcadTable.RecomputeTableBlock(true);

                /**********************************************************************/
                /* Add the label                                                     */
                /**********************************************************************/
                addTextEnt(pRecord, point + m_textOffset, point + m_textOffset, "ACAD_TABLE",
                  m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);
            }
        }

        /************************************************************************/
        /* Add a Text with Field to the specified BlockTableRecord              */
        /************************************************************************/
        void addTextWithField(ObjectId btrId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId, ObjectId noteStyleId)
        {
            BlockTableRecord pRecord = (BlockTableRecord)btrId.GetObject(OpenMode.ForWrite);

            Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
            double h = m_EntityBoxes.getHeight();
            double w = m_EntityBoxes.getWidth(boxRow, boxCol);

            //  double dx = w/16.0;
            //  double dy = h/12.0;

            Point3d textPos1 = point;
            textPos1 += new Vector3d(w / 15.0, -h / 3.0, 0);

            Point3d textPos2 = point;
            textPos2 += new Vector3d(w / 15.0, -2.0 * h / 3.0, 0);

            double textHeight = m_EntityBoxes.getHeight() / 12.0;

            /**********************************************************************/
            /* Prepare the text entities                                           */
            /**********************************************************************/
            DBText pText1 = new DBText();
            ObjectId textId = pRecord.AppendEntity(pText1);
            DBText pText2 = new DBText();
            ObjectId textId2 = pRecord.AppendEntity(pText2);

            pText1.Position = textPos1;
            pText1.Height = textHeight;
            pText2.Position = textPos2;
            pText2.Height = textHeight;
            if (!styleId.IsNull)
            {
                pText1.TextStyleId = styleId;
                pText2.TextStyleId = styleId;
            }

            /**********************************************************************/
            /* Create field objects                                               */
            /**********************************************************************/
            Field pTextField1 = new Field();
            Field pField1_1 = new Field();

            Field pTextField2 = new Field();
            Field pField2_1 = new Field();
            Field pField2_2 = new Field();

            /**********************************************************************/
            /* Set field objects                                                  */
            /**********************************************************************/
            ObjectId textFldId1 = pText1.SetField("TEXT", pTextField1);
            ObjectId fldId1_1 = pTextField1.SetField("", pField1_1);
            ObjectId textFldId2 = pText2.SetField("TEXT", pTextField2);

            /**********************************************************************/
            /* Set field property                                                 */
            /**********************************************************************/

            pField1_1.EvaluationOption = FieldEvaluationOptions.Automatic;
            string fc1 = "\\AcVar Comments";
            pField1_1.SetFieldCode(fc1);

            pTextField1.EvaluationOption = FieldEvaluationOptions.Automatic;
            string fc2 = "%<\\_FldIdx 0>%";
            FieldCodeWithChildren fcwChd = pTextField1.GetFieldCodeWithChildren();
            fcwChd.FieldCode = fc2;
            pTextField1.SetFieldCodeWithChildren(FieldCodeFlags.TextField | FieldCodeFlags.PreserveFields, fcwChd);

            /**********************************************************************/
            /* Evaluate field                                                     */
            /**********************************************************************/
            pField1_1.Evaluate(); // TODO:

            pTextField2.EvaluationOption = FieldEvaluationOptions.Automatic;
            string fc3 = "Date %<\\_FldIdx 0>% Time %<\\_FldIdx 1>%";

            fcwChd = pTextField2.GetFieldCodeWithChildren();
            fcwChd.Add(0, pField2_1);
            fcwChd.Add(1, pField2_2);
            fcwChd.FieldCode = fc3;
            pTextField2.SetFieldCodeWithChildren(FieldCodeFlags.TextField, fcwChd);

            pField2_1.EvaluationOption = FieldEvaluationOptions.Automatic;
            string fc4 = "\\AcVar Date \\f M/dd/yyyy";
            pField2_1.SetFieldCode(fc4);

            pField2_2.EvaluationOption = FieldEvaluationOptions.Automatic;
            string fc5 = "\\AcVar Date \\f h:mm tt";
            pField2_2.SetFieldCode(fc5);

            /**********************************************************************/
            /* Evaluate fields                                                    */
            /**********************************************************************/
            pField2_1.Evaluate();
            pField2_2.Evaluate();

            /**********************************************************************/
            /* Add the label                                                      */
            /**********************************************************************/
            addTextEnt(pRecord, point + m_textOffset, point + m_textOffset, "FIELDS",
              m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, noteStyleId);
        }

        /************************************************************************/
        /* Prefix a file name with the Current Directory                        */
        /************************************************************************/
        string inCurrentFolder(string fileName)
        {
            return Directory.GetCurrentDirectory() + "\\" + Path.GetFileName(fileName);
        }

        void addDwfUnderlay(ObjectId blockId, int boxRow, int boxCol, ObjectId layerId, ObjectId styleId)
        {
            /**********************************************************************/
            /* Open the BlockTableRecord                                          */
            /**********************************************************************/
            using (BlockTableRecord bBTR = (BlockTableRecord)blockId.GetObject(OpenMode.ForWrite))
            {

                /**********************************************************************/
                /* Get the Upper-left corner of the box and its size                  */
                /**********************************************************************/
                Point3d point = m_EntityBoxes.getBox(boxRow, boxCol);
                double h = m_EntityBoxes.getHeight();
                double w = m_EntityBoxes.getWidth(boxRow, boxCol);

                /**********************************************************************/
                /* Add the label                                                      */
                /**********************************************************************/
                addTextEnt(bBTR, point + m_textOffset, point + m_textOffset,
                  "Dwf reference", m_textSize, TextHorizontalMode.TextLeft, TextVerticalMode.TextTop, layerId, styleId);

                /**********************************************************************/
                /* Create the Dwf definition                                          */
                /**********************************************************************/
                using (DwfDefinition pDwfDef = new DwfDefinition())
                {
                    string itemName = "Unsaved Drawing-Mode";
                    pDwfDef.SourceFileName = "OdWriteEx.dwf";
                    pDwfDef.ItemName = itemName;

                    // Post to database
                    ObjectId idDef = ObjectId.Null;
                    {
                        string dictName = UnderlayDefinition.GetDictionaryKey(pDwfDef.GetType());

                        Database pDb = blockId.Database;
                        DBDictionary pDict = (DBDictionary)pDb.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite);
                        ObjectId idDefDict = pDict.GetAt(dictName);
                        if (idDefDict.IsNull)
                        {
                            idDefDict = pDict.SetAt(dictName, new DBDictionary());
                        }

                        DBDictionary pDefs = (DBDictionary)idDefDict.GetObject(OpenMode.ForWrite);
                        idDef = pDefs.SetAt("OdWriteEx - " + itemName, pDwfDef);
                    }
                    /**********************************************************************/
                    /* Create the Dwf reference                                           */
                    /**********************************************************************/
                    DwfReference pDwfRef = new DwfReference();
                    pDwfRef.SetDatabaseDefaults(bBTR.Database);

                    /**********************************************************************/
                    /* Add the Dwf reference to the database                              */
                    /**********************************************************************/
                    bBTR.AppendEntity(pDwfRef);

                    /**********************************************************************/
                    /* Set the Dwf reference's parameters                                 */
                    /**********************************************************************/
                    pDwfRef.DefinitionId = idDef;
                    pDwfRef.Position = point + new Vector3d(-w / 4, -h / 2, 0.0);
                    pDwfRef.ScaleFactors = new Scale3d(0.001);
                }
            }
        }
    }

    class EntityBoxes
    {
        public const double WIDTH_BOX = 2.25;
        public const double HEIGHT_BOX = 3.25;

        public const double HOR_SPACE = 0.625;
        public const double VER_SPACE = 0.375;

        public const int HOR_BOXES = 11;
        public const int VER_BOXES = 7;

        static readonly int[,] BoxSizes = new int[VER_BOXES, HOR_BOXES]
      {
        {1,1,1,1,2,1,1,1,1,1,0},
        {1,3,2,1,1,1,2,0,0,0,0},
        {2,3,3,1,2,0,0,0,0,0,0},
        {1,1,1,2,1,1,1,1,1,1,0},
        {2,2,2,1,1,2,1,0,0,0,0},
        {3,2,1,1,1,1,1,1,0,0,0},
        {1,1,1,1,1,1,1,1,1,1,1}
      };

        public EntityBoxes()
        {
        }

        /**********************************************************************/
        /* Return the width of the specified box                              */
        /**********************************************************************/
        public double getWidth(int row, int col)
        {
            return BoxSizes[row, col] * WIDTH_BOX + (BoxSizes[row, col] - 1) * HOR_SPACE;
        }
        /**********************************************************************/
        /* Return the height of specified box                                 */
        /**********************************************************************/
        public double getHeight()
        {
            return HEIGHT_BOX;
        }
        /**********************************************************************/
        /* Return true if and only if the specified box is a box              */
        /**********************************************************************/
        public bool isBox(int row, int col)
        {
            return BoxSizes[row, col] > 0 ? true : false;
        }

        /**********************************************************************/
        /* Return the upper-left corner of the specified box                  */
        /**********************************************************************/
        public Point3d getBox(int row, int col)
        {
            Point3d point = new Point3d();
            if (col > HOR_BOXES - 1)
                return point;

            point = new Point3d(0, HEIGHT_BOX * VER_BOXES + VER_SPACE * (VER_BOXES - 1), 0);
            for (int i = 0; i < col; i++)
            {
                point = point.TransformBy(Matrix3d.Displacement(Vector3d.XAxis * (BoxSizes[row, i] * WIDTH_BOX + BoxSizes[row, i] * HOR_SPACE)));
            }
            point = point.TransformBy(Matrix3d.Displacement(-Vector3d.YAxis * (row * HEIGHT_BOX + row * VER_SPACE)));
            return point;
        }

        /**********************************************************************/
        /* Return the center of the specified box                             */
        /**********************************************************************/
        public Point3d getBoxCenter(int row, int col)
        {
            Point3d point = getBox(row, col);
            double w = getWidth(row, col);

            point = point.TransformBy(Matrix3d.Displacement(Vector3d.XAxis * w / 2.0 - Vector3d.YAxis * HEIGHT_BOX / 2.0));
            return point;
        }

        /**********************************************************************/
        /* Return the size of the box array                                   */
        /**********************************************************************/
        public Vector3d getArraySize()
        {
            return new Vector3d(WIDTH_BOX * HOR_BOXES + HOR_SPACE * (HOR_BOXES - 1),
                               -(HEIGHT_BOX * VER_BOXES + VER_SPACE * (VER_BOXES - 1)),
                               0);
        }

        /**********************************************************************/
        /* Return the center of the box array                                 */
        /**********************************************************************/
        public Point3d getArrayCenter()
        {
            return new Point3d(0.5 * (WIDTH_BOX * HOR_BOXES + HOR_SPACE * (HOR_BOXES - 1)),
                               0.5 * (HEIGHT_BOX * VER_BOXES + VER_SPACE * (VER_BOXES - 1)),
                               0);
        }
    }
}
