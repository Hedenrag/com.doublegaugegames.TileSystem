using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DGG
{
    namespace Tiles
    {
        namespace Editor
        {


            internal class CustomHandles
            {
                private static Vector2 s_StartMousePosition;

                private static Vector2 s_CurrentMousePosition;

                private static Vector2 s_CurrentMousePositionScreen;

                private static Vector3 s_StartPosition;
                public static Vector3 MoveHandle(Vector3 position, Quaternion rotation, float size, Vector3 snap, Handles.CapFunction handleFunction)
                {
                    return MoveHandle(position, rotation, size, snap, handleFunction, Color.white);
                }
                public static Vector3 MoveHandle(Vector3 position, Quaternion rotation, float size, Vector3 snap, Handles.CapFunction handleFunction, Color color)
                {
                    int id = GUIUtility.GetControlID("FreeMoveHandleHash".GetHashCode(), FocusType.Passive);
                    Vector3 position2 = Handles.matrix.MultiplyPoint(position);
                    Matrix4x4 matrix = Handles.matrix;
                    //VertexSnapping.HandleMouseMove(id);
                    Event current = Event.current;
                    switch (current.GetTypeForControl(id))
                    {
                        case EventType.MouseMove:
                        case EventType.Layout:
                            Handles.matrix = Matrix4x4.identity;
                            handleFunction(id, position2, rotation, size, EventType.Layout);
                            Handles.matrix = matrix;
                            break;
                        case EventType.MouseDown:
                            if (HandleUtility.nearestControl == id && current.button == 0)
                            {
                                GUIUtility.hotControl = id;
                                s_CurrentMousePosition = (s_CurrentMousePositionScreen = (s_StartMousePosition = current.mousePosition));
                                s_StartPosition = position;
                                //HandleUtility.ignoreRaySnapObjects = null;
                                current.Use();
                                EditorGUIUtility.SetWantsMouseJumping(1);
                            }

                            break;
                        case EventType.MouseDrag:
                            {
                                if (GUIUtility.hotControl != id)
                                {
                                    break;
                                }

                                bool flag = EditorGUI.actionKey && current.shift;
                                if (flag)
                                {
                                    //if (HandleUtility.ignoreRaySnapObjects == null)
                                    //{
                                    //    Handles.SetupIgnoreRaySnapObjects();
                                    //}

                                    if (HandleUtility.PlaceObject(current.mousePosition, out var position3, out var normal))
                                    {
                                        float num = 0f;
                                        //if (Tools.pivotMode == PivotMode.Center && !Tools.vertexDragging)
                                        //{
                                        //    float num2 = HandleUtility.CalcRayPlaceOffset(HandleUtility.ignoreRaySnapObjects, normal);
                                        //    if (num2 != float.PositiveInfinity)
                                        //    {
                                        //        num = Vector3.Dot(position, normal) - num2;
                                        //    }
                                        //}

                                        position = Handles.inverseMatrix.MultiplyPoint(position3 + normal * num);
                                    }
                                    else
                                    {
                                        flag = false;
                                    }
                                }

                                if (!flag)
                                {
                                    Vector2 vector = current.mousePosition - s_CurrentMousePositionScreen;
                                    s_CurrentMousePositionScreen += vector;
                                    s_CurrentMousePosition += new Vector2(vector.x, 0f - vector.y) * EditorGUIUtility.pixelsPerPoint;
                                    Vector3 position4 = Camera.current.WorldToScreenPoint(Handles.matrix.MultiplyPoint(s_StartPosition));
                                    position4 += (Vector3)(s_CurrentMousePosition - s_StartMousePosition);
                                    position = Handles.inverseMatrix.MultiplyPoint(Camera.current.ScreenToWorldPoint(position4));
                                    //if (Tools.vertexDragging)
                                    //{
                                    //    if (HandleUtility.ignoreRaySnapObjects == null)
                                    //    {
                                    //        Handles.SetupIgnoreRaySnapObjects();
                                    //    }
                                    //
                                    //    if (HandleUtility.FindNearestVertex(current.mousePosition, null, out var vertex))
                                    //    {
                                    //        position = Handles.inverseMatrix.MultiplyPoint(vertex);
                                    //    }
                                    //}
                                    //else 
                                    if (EditorSnapSettings.incrementalSnapActive && !current.shift)
                                    {
                                        Vector3 value = position - s_StartPosition;
                                        value = Handles.SnapValue(value, snap);
                                        position = s_StartPosition + value;
                                    }
                                }

                                GUI.changed = true;
                                current.Use();
                                break;
                            }
                        case EventType.MouseUp:
                            if (GUIUtility.hotControl == id && (current.button == 0 || current.button == 2))
                            {
                                GUIUtility.hotControl = 0;
                                //HandleUtility.ignoreRaySnapObjects = null;
                                current.Use();
                                EditorGUIUtility.SetWantsMouseJumping(0);
                            }

                            break;
                        case EventType.Repaint:
                            {
                                //Handles.SetupHandleColor(id, current, out var prevColor, out var _);
                                color = SelectColor(id, color, current);
                                Handles.color = color;
                                Handles.matrix = Matrix4x4.identity;
                                handleFunction(id, position2, rotation, size, EventType.Repaint);
                                Handles.matrix = matrix;
                                break;
                            }
                    }

                    return position;
                }

                readonly static Color selectedColor = Color.yellow;
                readonly static Color s_HoverIntensity = new Color(1.1f, 1.1f, 1.1f, 1.33f);
                static Color SelectColor(int ID, Color baseColor, Event evt)
                {
                    if (ID == GUIUtility.hotControl)
                    {
                        return selectedColor;
                    }
                    else if (IsHovering(ID, evt))
                    {
                        Color color = baseColor * s_HoverIntensity;
                        color.r = Mathf.Clamp01(color.r);
                        color.g = Mathf.Clamp01(color.g);
                        color.b = Mathf.Clamp01(color.b);
                        color.a = Mathf.Clamp01(color.a);
                        return color;
                    }
                    return baseColor;
                    static bool IsHovering(int controlID, Event evt)
                    {
                        return controlID == HandleUtility.nearestControl && GUIUtility.hotControl == 0 && !Tools.viewToolActive;
                    }
                }
            }
        }
    }
}