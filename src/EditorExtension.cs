using GrxCAD.DatabaseServices;
using GrxCAD.EditorInput;
using GrxCAD.Geometry;
using GrxCAD.Runtime;

namespace Sharper.GstarCAD.Extensions.Geometry
{
    /// <summary>
    /// Provides extension methods for the Editor type.
    /// </summary>
    public static class EditorExtension
    {
        /// <summary>
        /// Gets the transformation matrix from the current User Coordinate System (UCS) to the World Coordinate System (WCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The UCS to WCS transformation matrix.</returns>
        public static Matrix3d UcsToWcs(this Editor editor)
        {
            return editor.CurrentUserCoordinateSystem;
        }

        /// <summary>
        /// Gets the transformation matrix from the World Coordinate System (WCS) to the current User Coordinate System (UCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The WCS to UCS transformation matrix.</returns>
        public static Matrix3d WcsToUcs(this Editor editor)
        {
            return editor.CurrentUserCoordinateSystem.Inverse();
        }

        /// <summary>
        /// Gets the transformation matrix from the current viewport Display Coordinate System (DCS) to the World Coordinate System (WCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The DCS to WCS transformation matrix.</returns>
        public static Matrix3d DcsToWcs(this Editor editor)
        {
            Matrix3d retVal;
            bool tileMode = editor.Document.Database.TileMode;
            if (!tileMode)
                editor.SwitchToModelSpace();
            using (ViewTableRecord vtr = editor.GetCurrentView())
            {
                retVal =
                    Matrix3d.Rotation(-vtr.ViewTwist, vtr.ViewDirection, vtr.Target) *
                    Matrix3d.Displacement(vtr.Target - Point3d.Origin) *
                    Matrix3d.PlaneToWorld(vtr.ViewDirection);
            }

            if (!tileMode)
                editor.SwitchToPaperSpace();
            return retVal;
        }

        /// <summary>
        /// Gets the transformation matrix from the World Coordinate System (WCS) to the current viewport Display Coordinate System (DCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The WCS to DCS transformation matrix.</returns>
        public static Matrix3d WcsToDcs(this Editor editor)
        {
            return editor.DcsToWcs().Inverse();
        }

        /// <summary>
        ///  Gets the transformation matrix from the paper space active viewport Display Coordinate System (DCS) to the Paper space Display Coordinate System (PSDCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The DCS to PSDCS transformation matrix.</returns>
        /// <exception cref=" GrxCAD.Runtime.Exception">
        /// eNotInPaperSpace is thrown if this method is called form Model Space.</exception>
        /// <exception cref=" GrxCAD.Runtime.Exception">
        /// eCannotChangeActiveViewport is thrown if there is none floating viewport in the current layout.
        /// </exception>
        public static Matrix3d DcsToPsdcs(this Editor editor)
        {
            Database db = editor.Document.Database;
            if (db.TileMode)
                throw new Exception(ErrorStatus.NotInPaperspace);
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Viewport viewport =
                    (Viewport)tr.GetObject(editor.CurrentViewportObjectId, OpenMode.ForRead);
                if (viewport.Number != 1) return viewport.DcsToPsdcs();
                try
                {
                    editor.SwitchToModelSpace();
                    viewport = (Viewport)tr.GetObject(editor.CurrentViewportObjectId, OpenMode.ForRead);
                    editor.SwitchToPaperSpace();
                }
                catch
                {
                    throw new Exception(ErrorStatus.CannotChangeActiveViewport);
                }

                return viewport.DcsToPsdcs();
            }
        }

        /// <summary>
        ///  Gets the transformation matrix from the Paper space Display Coordinate System (PSDCS) to the paper space active viewport Display Coordinate System (DCS).
        /// </summary>
        /// <param name="editor">The instance to which this method applies.</param>
        /// <returns>The PSDCS to DCS transformation matrix.</returns>
        /// <exception cref=" GrxCAD.Runtime.Exception">
        /// eNotInPaperSpace is thrown if this method is called form Model Space.</exception>
        /// <exception cref=" GrxCAD.Runtime.Exception">
        /// eCannotChangeActiveViewport is thrown if there is none floating viewport in the current layout.
        /// </exception>
        public static Matrix3d PsdcsToDcs(this Editor editor)
        {
            return editor.DcsToPsdcs().Inverse();
        }
    }
}
