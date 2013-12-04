﻿#pragma strict

public class OGScrollView extends OGWidget {
	public var scrollWindow : Vector2;
	public var scrollPosition : Vector2;
	public var padding : Vector2 = new Vector2 ( 10, 10 );

	private function GetBounds ( w : OGWidget ) : Vector4 {
		var outside : Vector4 = Vector4.zero;
		
		var xMin : float = w.transform.position.x + w.scrollOffset.x;
		var xMax : float = w.transform.position.x + w.scrollOffset.x + w.transform.lossyScale.x;
		var yMin : float = w.transform.position.y + w.scrollOffset.y;
		var yMax : float = w.transform.position.y + w.scrollOffset.y + w.transform.lossyScale.y;

		if ( yMin < this.transform.position.y + padding.y ) {
			outside.z = this.transform.position.y - yMin;
		} else if ( yMax > this.transform.position.y + scrollWindow.y ) {
			outside.w = yMax - ( this.transform.position.y + scrollWindow.y );
		}
		
		if ( xMin < this.transform.position.x ) {
			outside.x = xMin - this.transform.position.x;
		} else if ( xMax > this.transform.position.x + scrollWindow.x ) {
			outside.y = xMax - this.transform.position.x + scrollWindow.x;
		}
		
		return outside;
	}

	private function IsOutOfBounds ( w : OGWidget ) {
		var outBounds : Vector4 = GetBounds ( w );
		return outBounds.z >= w.transform.lossyScale.y || outBounds.w > w.transform.lossyScale.y;
	}

	override function UpdateWidget () {
		if ( stretch.width != ScreenSize.None ) {
			scrollWindow.x = RecalcScale().x * Screen.width;
		}

		if ( stretch.height != ScreenSize.None ) {
			scrollWindow.y = RecalcScale().y * Screen.height;
		}
	
		this.transform.localScale = Vector3.one;
		
		var scroll : float = Input.GetAxis ( "Mouse ScrollWheel" );
		var drag : Vector2;
		drag.x = Input.GetAxis ( "Mouse X" ); 
		drag.y = Input.GetAxis ( "Mouse Y" );
		
		if ( CheckMouseOver ( drawRct ) ) {
			var amount : Vector2;
			
			if ( scroll > 0 ) {
				amount.y = 20;
			
			} else if ( scroll < 0 ) {
				amount.y = -20;
			
			} else if ( Input.GetMouseButton ( 2 ) ) {
				amount.x = Mathf.Floor ( drag.x * 20 );
				amount.y = -Mathf.Floor ( drag.y * 20 );
			
			}
			
			//if ( scrollPosition.y + amount.y > 0 || scrollPosition.x + amount.x > 0 ) { return; }
			
			scrollPosition += amount;
		}
		
		for ( var w : OGWidget in this.gameObject.GetComponentsInChildren.<OGWidget>() ) {
			if ( w != this ) {
				w.scrollOffset = new Vector3 ( padding.x + scrollPosition.x, padding.y + scrollPosition.y, 0 );
				w.drawDepth -= drawDepth;
				w.clipping = GetBounds ( w );		
				w.isDrawn = !IsOutOfBounds ( w );
			}
		}
	}
	
	override function DrawGL () {
		GL.TexCoord2 ( drawCrd.x, drawCrd.y );
		GL.Vertex3 ( drawRct.x, drawRct.y, -this.transform.position.z );
		
		GL.TexCoord2 ( drawCrd.x, drawCrd.y + drawCrd.height );
		GL.Vertex3 ( drawRct.x, drawRct.y + drawRct.height, -this.transform.position.z );
		
		GL.TexCoord2 ( drawCrd.x + drawCrd.width, drawCrd.y + drawCrd.height );
		GL.Vertex3 ( drawRct.x + drawRct.width, drawRct.y + drawRct.height, -this.transform.position.z );
		
		GL.TexCoord2 ( drawCrd.x + drawCrd.width, drawCrd.y );
		GL.Vertex3 ( drawRct.x + drawRct.width, drawRct.y, -this.transform.position.z );
	}
}
