using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public delegate void OGDelegate ( params object[] parameters ); 

[System.Serializable]
public class OGLine {
	public OGWidget start;
	public Vector3 startDir;
	public OGWidget end;
	public Vector3 endDir;
	public int segments;

	public OGLine ( OGWidget start, Vector3 startDir, OGWidget end, Vector3 endDir, int segments ) {
		this.start = start;
		this.startDir = startDir;
		this.end = end;
		this.endDir = endDir;
		this.segments = segments;
	}
}

[ExecuteInEditMode]
public class OGRoot : MonoBehaviour {
	
	public static OGRoot instance;

	public Vector2 targetResolution;
	public OGSkin skin;
	public OGPage[] currentPages = new OGPage [ 0 ];
	public Material lineMaterial;
	public OGWidget downWidget;	
	public bool isMouseOver = false;

	private OGWidget[] widgets;
	private List< OGWidget > mouseOver = new List< OGWidget > ();
	private Texture2D guiTex;
	private Camera cam;


	//////////////////
	// Instance
	//////////////////
	public static OGRoot GetInstance () {
		return instance;
	}

	public OGPage currentPage {
		get {
			if ( currentPages.Length > 0 ) {
				return currentPages [ 0 ];	

			} else {
				return null;
			
			}
		}
	}

	public Vector2 ratio {
		get {
			Vector2 result = Vector2.one;
			
			result.x = Screen.width / screenWidth;
			result.y = Screen.height / screenHeight;

			return result;
		}
	}
	
	public Vector2 reverseRatio {
		get {
			Vector2 result = Vector2.one;
			
			result.x = screenWidth / Screen.width;
			result.y = screenHeight / Screen.height;

			return result;
		}
	}
	
	public float screenWidth {
		get {
			if ( targetResolution.x > 0 ) {
				return targetResolution.x;
			
			} else if ( targetResolution.y > 0 ) {
				float ratio = targetResolution.y / Screen.height;
				
				return Screen.width * ratio;

			} else {
				return Screen.width;

			}
		}
	}
	
	public float screenHeight {
		get {
			if ( targetResolution.y > 0 ) {
				return targetResolution.y;
			
			} else if ( targetResolution.x > 0 ) {
				float ratio = targetResolution.x / Screen.width;
				
				return Screen.height * ratio;

			} else {
				return Screen.height;

			}
		}
	}


	//////////////////
	// Page management
	//////////////////
	public void RemoveFromCurrentPages ( OGPage page ) {
		List< OGPage > pages = new List< OGPage > ( currentPages );

		pages.Remove ( page );

		currentPages = pages.ToArray ();
	}
	
	public void AddToCurrentPages ( OGPage page ) {
		List< OGPage > pages = new List< OGPage > ( currentPages );

		pages.Add ( page );

		currentPages = pages.ToArray ();
	}
	
	public void SetCurrentPages ( OGPage[] pages ) {
		currentPages = pages;
		
		foreach ( OGPage p in this.GetComponentsInChildren<OGPage>(true) ) {
			foreach ( OGPage cp in pages ) {	
				if ( p == cp ) {
					p.gameObject.SetActive ( true );
					p.UpdateStyles ();
					
					if ( Application.isPlaying ) {
						p.StartPage ();
					}

				} else if ( p.gameObject.activeSelf ) {
					if ( Application.isPlaying ) {
						p.ExitPage ();
					}

					p.gameObject.SetActive ( false );
				}
			}
		}
	}
	
	public void SetCurrentPage ( OGPage page ) {
	       	currentPages = new OGPage [] { page };

		foreach ( OGPage p in this.GetComponentsInChildren<OGPage>(true) ) {
			if ( p == page ) {
				p.gameObject.SetActive ( true );
				p.UpdateStyles ();
				
				if ( Application.isPlaying ) {
					p.StartPage ();
				}

			} else if ( p.gameObject.activeSelf ) {
				if ( Application.isPlaying ) {
					p.ExitPage ();
				}

				p.gameObject.SetActive ( false );

			}
		}
	}

	public void GoToPages ( params string[] pageNames ) {
		List< OGPage > pages = new List< OGPage > ();
		
		foreach ( OGPage p in currentPages ) {
			p.ExitPage ();

			p.gameObject.SetActive ( false );
		}

		foreach ( OGPage p in this.GetComponentsInChildren<OGPage>(true) ) {
			foreach ( string n in pageNames ) {	
				if ( p.pageName == n ) {
					pages.Add ( p );
				}
			}
		}

		SetCurrentPages ( pages.ToArray () );
	}

	public void GoToPage ( string pageName ) {
		foreach ( OGPage p in this.GetComponentsInChildren<OGPage>(true) ) {
			if ( p.pageName == pageName ) {
				SetCurrentPage ( p );
			}
		}
		
		if ( currentPage != null ) {
			currentPage.gameObject.SetActive ( true );
		
		}
	}

	
	//////////////////
	// Draw loop
	//////////////////
	public void OnPostRender () {
		if ( skin != null && widgets != null ) {
			int i = 0;
			int o = 0;
			OGWidget w;
			
			GL.PushMatrix();
			GL.LoadPixelMatrix ( 0, screenWidth, 0, screenHeight );

			// Draw skin
			GL.Begin(GL.QUADS);
			OGDrawHelper.SetPass(skin.atlas);
			
			for ( i = 0; i < widgets.Length; i++ ) {
				w = widgets[i];
				
				if ( w == null ) {
					continue;
				
				} else if ( w.drawRct.x == 0 && w.drawRct.y == 0 && w.drawRct.height == 0 && w.drawRct.width == 0 ) {
					w.Recalculate ();
					continue;
				}
				
				if ( w.currentStyle == null ) {
					w.currentStyle = w.styles.basic;
				}
				
				if ( w.gameObject.activeSelf && w.isDrawn && w.drawRct.height > 0 && w.drawRct.width > 0 ) {
					w.DrawSkin ();
				}
			}
			
			GL.End ();
			
			// Draw text
			for ( i = 0; i < skin.fonts.Length; i++ ) {
				if ( skin.fonts[0] == null ) { continue; }
				
				GL.Begin(GL.QUADS);
				
				if ( skin.fontShader != null && skin.fonts[i].bitmapFont != null ) {
					skin.fonts[i].bitmapFont.material.shader = skin.fontShader;
				}
				
				if ( skin.fonts[i].bitmapFont != null ) {
					OGDrawHelper.SetPass ( skin.fonts[i].bitmapFont.material );
				}

				for ( o = 0; o < widgets.Length; o++ ) {
					w = widgets[o];
				
					if ( w == null ) { continue; }

					if ( w.styles == null ) {
						skin.ApplyDefaultStyles ( w );

					} else if ( w.isDrawn && w.gameObject.activeSelf ) {
						if ( w.currentStyle != null && w.currentStyle.text.fontIndex == i ) {
							if ( w.currentStyle.text.font == null ) {
								w.currentStyle.text.font = skin.fonts[i];
							}
							
							w.DrawText ();
						}
					}
				}
				
				GL.End ();
			}
			
			// Draw lines
			if ( lineMaterial != null ) {
				GL.Begin(GL.LINES);
				lineMaterial.SetPass(0);
					
				for ( i = 0; i < widgets.Length; i++ ) {	
					w = widgets[i];
					
					if ( w != null && w.gameObject.activeSelf && w.isDrawn ) {
						w.DrawLine();
					}
				}
				
				GL.End();
			}
			
			// Draw textures
			for ( i = 0; i < widgets.Length; i++ ) {	
				w = widgets[i];
				
				if ( w != null && w.gameObject.activeSelf && w.isDrawn ) {
					w.DrawGL();
				}
			}


			GL.PopMatrix();
		}
	}
	
	
	//////////////////
	// Init
	//////////////////
	public void Awake () {
		instance = this;
	}
	
	public void Start () {
		if ( currentPage != null && Application.isPlaying ) {
			currentPage.StartPage ();
		}
	}


	//////////////////
	// Update
	//////////////////
	public void ReleaseWidget () {
		downWidget = null;
	}

	private OGWidget [] GetCurrentWidgets () {
		List< OGWidget > list = new List< OGWidget > ();

		for ( int i = 0; i < currentPages.Length; i++ ) {
			OGPage page = currentPages [ i ];
			OGWidget[] ws = page.gameObject.GetComponentsInChildren<OGWidget>();
			
			for ( int w = 0; w < ws.Length; w++ ) {
				list.Add ( ws [ w ] );
			}
		}

		return list.OrderByDescending ( (w) => w.transform.position.z ).ToArray ();
	}

	public void Update () {
		if ( instance == null ) {
			instance = this;
		}

		if ( !cam ) {
			cam = this.gameObject.GetComponent< Camera > ();
			
			if ( !cam ) {
				cam = this.gameObject.AddComponent< Camera > ();
			}
		}

		cam.hideFlags = HideFlags.None;
		cam.cullingMask = 1 << this.gameObject.layer;
		cam.clearFlags = CameraClearFlags.Depth;

		if ( Camera.main ) {
			cam.depth = Camera.main.depth + 5;
		}

		this.transform.localScale = Vector3.one;
		this.transform.localPosition = Vector3.zero;
		this.transform.localEulerAngles = Vector3.zero;

		// Dirty
		UpdateWidgets ();

		// Only update these when playing
		if ( Application.isPlaying && currentPage != null ) {
			// Current page
			currentPage.UpdatePage ();

			// Update styles if in edit mode
			if ( !Application.isPlaying ) {
				currentPage.UpdateStyles ();
			}

			// Mouse interaction
			UpdateMouse ();	
		}

		// Force OGPage transformation
		if ( currentPage ) {
			currentPage.gameObject.layer = this.gameObject.layer;
			
			currentPage.transform.localScale = Vector3.one;
			currentPage.transform.localPosition = Vector3.zero;
			currentPage.transform.localEulerAngles = Vector3.zero;
		}
	}


	//////////////////
	// Mouse interaction
	//////////////////
	private Vector2 GetDragging () {
		Vector2 dragging = Vector2.zero;
		
		if ( Input.GetMouseButton ( 0 ) || Input.GetMouseButton ( 2 ) ) {
			dragging.x = Input.GetAxis ( "Mouse X" );
			dragging.y = Input.GetAxis ( "Mouse Y" );
		
		} else if ( GetTouch () == TouchPhase.Moved ) {
			Touch t = Input.GetTouch ( 0 );
			dragging = t.deltaPosition * ( Time.deltaTime / t.deltaTime );
		}

		return dragging;
	}
	
	private TouchPhase GetTouch () {
		if ( Input.touchCount < 1 ) {
			return (TouchPhase)(-1);
		
		} else {
			Touch touch = Input.GetTouch ( 0 );

			return touch.phase;
		}
	}
	
	public void UpdateMouse () {
		if ( widgets == null ) { return; }
		
		int i = 0;
		OGWidget w;

		// Click
		if ( Input.GetMouseButtonDown ( 0 ) || Input.GetMouseButtonDown ( 2 ) || GetTouch () == TouchPhase.Began ) {
			OGWidget topWidget = null;
			
			for ( i = 0; i < mouseOver.Count; i++ ) {
				w = mouseOver[i];
				
				if ( ( w.GetType() != typeof ( OGScrollView ) || ( w as OGScrollView ).touchControl ) && ( topWidget == null || w.transform.position.z < topWidget.transform.position.z ) && w.isSelectable ) {
					topWidget = w;
				}
			}
			
			if ( downWidget && downWidget != topWidget ) {
				downWidget.OnMouseCancel ();
			}
			
			if ( topWidget != null && topWidget.CheckMouseOver() && !topWidget.isDisabled ) {
				topWidget.OnMouseDown ();
				downWidget = topWidget;
			}

		// Release
		} else if ( Input.GetMouseButtonUp ( 0 ) || Input.GetMouseButtonUp ( 2 ) || GetTouch () == TouchPhase.Ended || GetTouch () == TouchPhase.Canceled ) {
			if ( downWidget ) {
				// Draggable
				if ( downWidget.resetAfterDrag && downWidget.GetType() != typeof ( OGScrollView ) ) {
					downWidget.transform.position = downWidget.dragOrigPos;
					downWidget.dragOffset = Vector3.zero;
					downWidget.dragOrigPos = Vector3.zero;
				}
				
				// Mouse over
				if ( ( downWidget.CheckMouseOver() || GetTouch () == TouchPhase.Ended ) && !downWidget.isDisabled && downWidget.CheckMouseOver()) {
					downWidget.OnMouseUp ();

				// Mouse out
				} else {
					downWidget.OnMouseCancel ();
				
				}
			
			}
		
		// Dragging
		} else if ( GetDragging () != Vector2.zero ) {
			if ( downWidget != null && !downWidget.isDisabled ) {
				if ( downWidget.clipTo && downWidget.clipTo.GetType() == typeof ( OGScrollView ) && ( downWidget.clipTo as OGScrollView ).touchControl ) {
					OGWidget thisWidget = downWidget;
					thisWidget.OnMouseCancel ();
					downWidget = thisWidget.clipTo;
				}
				
				downWidget.OnMouseDrag ();
			
				if ( downWidget.isDraggable && downWidget.GetType() != typeof ( OGScrollView ) ) {
					Vector3 mousePos = Input.mousePosition;
					mousePos.y = screenHeight - mousePos.y;

					if ( downWidget.dragOffset == Vector3.zero ) {
						if ( downWidget.resetAfterDrag ) {
							downWidget.dragOrigPos = downWidget.transform.position;
						}

						downWidget.dragOffset = downWidget.transform.position - mousePos;
					}

					Vector3 newPos = downWidget.transform.position;
					newPos = mousePos + downWidget.dragOffset;
					downWidget.transform.position = newPos;
				}
			}
		}

		// Escape key
		if ( Input.GetKeyDown ( KeyCode.Escape ) ) {
			if ( downWidget != null ) {
				downWidget.OnMouseCancel ();
				ReleaseWidget ();
			}
		}
	}	

	// OnGUI selection
	#if UNITY_EDITOR
	
	public static System.Action<OGWidget, bool> EditorSelectWidget = null;

	private OGWidget FindMouseOverWidget ( Event e ) {
		Vector2 pos = new Vector2 ( e.mousePosition.x * reverseRatio.x, screenHeight - e.mousePosition.y * reverseRatio.y );

		for ( int i = widgets.Length - 1; i >= 0; i-- ) {
			if ( widgets[i].drawRct.Contains ( pos ) ) {
				return widgets[i];
			}
		}

		return null;
	}

	private void MoveSelection ( float x, float y ) {
		for ( int i = 0; i < UnityEditor.Selection.gameObjects.Length; i++ ) {
			OGWidget w = UnityEditor.Selection.gameObjects[i].GetComponent<OGWidget>();

			if ( w ) {
				Vector3 newPos = new Vector3 ( x, y, 0 );

				w.transform.localPosition = w.transform.localPosition + newPos;
			}
		}
	}

	public void OnGUI () {
		Event e = Event.current;

		if ( !Application.isPlaying ) {
			Color color = Color.white;
			
			if ( !guiTex ) {		
				guiTex = new Texture2D ( 1, 1 );
				guiTex.SetPixel ( 0, 0, color );
				guiTex.Apply ();
			}

			GUIStyle style = new GUIStyle();
			style.normal.background = guiTex;
			OGWidget w = null;		

			for ( int i = 0; i < UnityEditor.Selection.gameObjects.Length; i++ ) {
				w = UnityEditor.Selection.gameObjects[i].GetComponent<OGWidget>();

				if ( w ) {
					Rect revRect = w.scaledRct;
					revRect.y = Screen.height - revRect.y - revRect.height;

					Rect pivotRect = new Rect ( w.transform.position.x - 2, w.transform.position.y - 2, 4, 4 );
					
					pivotRect.x *= ratio.x;
					pivotRect.width *= ratio.x;
					pivotRect.y *= ratio.y;
					pivotRect.height *= ratio.y;
				
					UnityEditor.Handles.color = color;

					// Draw outline
					UnityEditor.Handles.DrawPolyLine (
						new Vector3 ( revRect.xMin, revRect.yMin, 0 ),
						new Vector3 ( revRect.xMin, revRect.yMax, 0 ),
						new Vector3 ( revRect.xMax, revRect.yMax, 0 ),
						new Vector3 ( revRect.xMax, revRect.yMin, 0 ),
						new Vector3 ( revRect.xMin, revRect.yMin, 0 )
					);

					// Draw pivot
					GUI.Box ( pivotRect, "", style );
					
				}
			}

			switch ( e.type ) { 
				case EventType.MouseDown:
					w = FindMouseOverWidget ( e );

					EditorSelectWidget ( w, e.shift );
					
					break;

				case EventType.KeyDown:
					int modifier = 1;

					if ( e.shift ) {
						modifier = 10;
					}

					switch ( e.keyCode ) {
						case KeyCode.UpArrow:
							MoveSelection ( 0, -modifier );
							break;
						
						case KeyCode.DownArrow:
							MoveSelection ( 0, modifier );
							break;
						
						case KeyCode.LeftArrow:
							MoveSelection ( -modifier, 0 );
							break;
						
						case KeyCode.RightArrow:
							MoveSelection ( modifier, 0 );
							break;
					}

					break;
			}
		}
	}
	#endif


	//////////////////
	// Widget management
	//////////////////
	public void UpdateWidgets () {
		if ( currentPage == null ) { return; }
		
		mouseOver.Clear ();
		
		// Update widget lists	
		widgets = GetCurrentWidgets ();

		for ( int i = 0; i < widgets.Length; i++ ) {
			OGWidget w = widgets[i];

			if ( w == null || !w.isDrawn || w.isDisabled ) { continue; }

			// Check mouse
			if ( w.CheckMouseOver() ) 
			{
				w.OnMouseOver ();
				mouseOver.Add ( w );
			}
			
			// Check scroll offset
			if ( !w.clipTo ) {
				w.scrollOffset.x = 0;
				w.scrollOffset.y = 0;
			}

			w.root = this;
			w.gameObject.layer = this.gameObject.layer;
			w.UpdateWidget ();
			w.Recalculate ();

			// Cleanup from previous OpenGUI versions
			if ( w.hidden ) {
				DestroyImmediate ( w.gameObject );
			}
		}
		
		// Is mouse over anything?
		isMouseOver = mouseOver.Count > 0;
	}
}
