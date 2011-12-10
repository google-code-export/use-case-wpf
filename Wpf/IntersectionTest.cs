/*
Copyright 2011 Sebastian Krysmanski. All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are
permitted provided that the following conditions are met:

   1. Redistributions of source code must retain the above copyright notice, this list of
      conditions and the following disclaimer.

   2. Redistributions in binary form must reproduce the above copyright notice, this list
      of conditions and the following disclaimer in the documentation and/or other materials
      provided with the distribution.

THIS SOFTWARE IS PROVIDED BY SEBASTIAN KRYSMANSKI ``AS IS'' AND ANY EXPRESS OR IMPLIED
WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL SEBASTIAN KRYSMANSKI OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those of the
authors and should not be interpreted as representing official policies, either expressed
or implied, of Sebastian Krysmanski.
*/

using System.Windows;
using System.Windows.Media;

namespace Wpf
{
  /// <summary>
  /// Represents a quadrilateral (polygon with four points, eg. a rect or a square). Note that the points are supposed
  /// to be in counter-clockwise order (usually starting with the upper left corner).
  /// </summary>
  public struct Quadrilateral {
    public Point A { get; set; }
    public Point B { get; set; }
    public Point C { get; set; }
    public Point D { get; set; }

    public Quadrilateral(Rect rect) : this() {
      this.A = rect.TopLeft;
      this.B = rect.TopRight;
      this.C = rect.BottomRight;
      this.D = rect.BottomLeft;
    }

    /// <summary>
    /// Rotates the quadrilateral clockwise around the specified point.
    /// </summary>
    /// <param name="degrees">angle in degrees (clockwise)</param>
    /// <param name="center">center point around which the quadrilateral is rotated</param>
    public void Rotate(double degrees, Point center) {
      Matrix mat = new Matrix();
      // NOTE: Matrix.RotateAt() rotates clockwise.
      mat.RotateAt(degrees, center.X, center.Y);
      this.A = mat.Transform(this.A);
      this.B = mat.Transform(this.B);
      this.C = mat.Transform(this.C);
      this.D = mat.Transform(this.D);
    }
  }

  public static class IntersectionTest {
    public static bool CheckRectRectIntersection(Quadrilateral quad1, Rect quad2) {
      return CheckRectRectIntersection(quad1, new Quadrilateral(quad2));
    }

    /// <summary>
    /// Checks whether the two convex quadrilaterals intersect. Note that if one of the quadrilaterals is concave, the
    /// test may return a wrong result (so don't use them here).
    /// </summary>
    /// <returns>Returns <c>true</c> if both quadrilaterals intersect/overlap; returns <c>false</c> if they don't.
    /// </returns>
    public static bool CheckRectRectIntersection(Quadrilateral quad1, Quadrilateral quad2) {
      Point[] quad1Points = new Point[] { quad1.A, quad1.B, quad1.C, quad1.D };
      Point[] quad2Points = new Point[] { quad2.A, quad2.B, quad2.C, quad2.D };

      //
      // Check quad1 edges
      // 
      if (DoAxisSeparationTest(quad1.A, quad1.B, quad1.C, quad2Points)) {
        return false;
      }

      if (DoAxisSeparationTest(quad1.A, quad1.D, quad1.C, quad2Points)) {
        return false;
      }

      if (DoAxisSeparationTest(quad1.D, quad1.C, quad1.A, quad2Points)) {
        return false;
      }

      if (DoAxisSeparationTest(quad1.C, quad1.B, quad1.A, quad2Points)) {
        return false;
      }

      //
      // Check quad2 edges
      // 
      if (DoAxisSeparationTest(quad2.A, quad2.B, quad2.C, quad1Points)) {
        return false;
      }

      if (DoAxisSeparationTest(quad2.A, quad2.D, quad2.C, quad1Points)) {
        return false;
      }

      if (DoAxisSeparationTest(quad2.D, quad2.C, quad2.A, quad1Points)) {
        return false;
      }

      if (DoAxisSeparationTest(quad2.C, quad2.B, quad2.A, quad1Points)) {
        return false;
      }

      // If we found no separating axis, then the quadrilaterals intersect.
      return true;
    }

    /// <summary>
    /// Does axis separation test for a convex quadrilateral.
    /// </summary>
    /// <param name="x1">Defines together with x2 the edge of quad1 to be checked whether its a separating axis.</param>
    /// <param name="x2">Defines together with x1 the edge of quad1 to be checked whether its a separating axis.</param>
    /// <param name="x3">One of the remaining two points of quad1.</param>
    /// <param name="otherQuadPoints">The four points of the other quad.</param>
    /// <returns>Returns <c>true</c>, if the specified edge is a separating axis (and the quadrilaterals therefor don't 
    /// intersect). Returns <c>false</c>, if it's not a separating axis.</returns>
    private static bool DoAxisSeparationTest(Point x1, Point x2, Point x3, Point[] otherQuadPoints) {
      Vector vec = x2 - x1;
      Vector rotated = new Vector(-vec.Y, vec.X);

      bool refSide = (rotated.X * (x3.X - x1.X)
                    + rotated.Y * (x3.Y - x1.Y)) >= 0;

      foreach (Point pt in otherQuadPoints) {
        bool side = (rotated.X * (pt.X - x1.X) 
                   + rotated.Y * (pt.Y - x1.Y)) >= 0;
        if (side == refSide) {
          // At least one point of the other quad is one the same side as x3. Therefor the specified edge can't be a
          // separating axis anymore.
          return false;
        }
      }

      // All points of the other quad are on the other side of the edge. Therefor the edge is a separating axis and
      // the quads don't intersect.
      return true;
    }

    public static bool CheckLineRectIntersection(Point pt1, Point pt2, Rect rect) {
      if (rect.Contains(pt1) || rect.Contains(pt2)) {
        // If one of the points is inside the rect, it intersects/overlaps.
        return true;
      }

      // If both points are outside, check each edge for intersecting. First we check whether the line is parallel
      // to one of the four edges.

      if (pt1.X == pt2.X && pt1.X >= rect.Left && pt1.X <= rect.Right) {
        // Match vertical edge
        return true;
      }
      if (pt1.Y == pt2.Y && pt1.Y >= rect.Top && pt1.Y <= rect.Bottom) {
        // Match horizontal edge
        return true;
      }

      // Didn't match. Now check for "real" intersection. Note that if the line intersects with one edge, it must also
      // intersect with another one (since none of the points is inside of the rect). So we only need to check the first
      // three edges.
      return CheckLineLineIntersection(rect.TopLeft, rect.BottomLeft, pt1, pt2)  // left edge
          || CheckLineLineIntersection(rect.TopLeft, rect.TopRight, pt1, pt2)  // top edge
          || CheckLineLineIntersection(rect.TopRight, rect.BottomRight, pt1, pt2);  // right edge
    }

    /// <summary>
    /// Checks for intersection of two lines.
    /// </summary>
    public static bool CheckLineLineIntersection(Point line1Pt1, Point line1Pt2, Point line2Pt1, Point line2Pt2) {
      Vector b = line1Pt2 - line1Pt1;
      Vector d = line2Pt2 - line2Pt1;
      double bDotDPerp = b.X * d.Y - b.Y * d.X;

      // if b dot d == 0, it means the lines are parallel so have infinite intersection points
      if (bDotDPerp == 0) {
        return false;
      }

      Vector c = line2Pt1 - line1Pt1;
      double lineFactor = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
      if (lineFactor < 0 || lineFactor > 1) {
        return false;
      }

      lineFactor = (c.X * b.Y - c.Y * b.X) / bDotDPerp;
      if (lineFactor < 0 || lineFactor > 1) {
        return false;
      }

      return true;
    }
  }
}
