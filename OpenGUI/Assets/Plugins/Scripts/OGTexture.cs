using UnityEngine;
using System.Collections;

public class OGTexture : OGWidget {
	public Texture2D mainTexture;
	
	private Material material;

	override public void UpdateWidget () {
		
		mouseRct = drawRct;
	}

	override public void DrawGL () {
		if ( drawRct == new Rect() || mainTexture == null ) {
			return;
		}
	
		if ( material == null ) {
			material = new Material ( root.skin.atlas.shader );
			return;
		}

		if ( material.mainTexture != mainTexture ) {
			material.mainTexture = mainTexture;
			return;
		}

		Rect displayRct = drawRct;
		Rect uvRct = drawCrd;

		if ( clipTo ) {
			if ( displayRct.xMin > clipTo.drawRct.xMax || displayRct.xMax < clipTo.drawRct.xMin || displayRct.yMax < clipTo.drawRct.yMin || displayRct.yMin > clipTo.drawRct.yMax ) {
				return;
			}

			if ( clipTo.drawRct.xMin > displayRct.xMin ) {
				uvRct.xMin = ( clipTo.drawRct.xMin - displayRct.xMin ) / this.transform.lossyScale.x;
				displayRct.xMin = clipTo.drawRct.xMin;
			}
			
			if ( clipTo.drawRct.xMax < displayRct.xMax ) {
				uvRct.xMax = ( displayRct.xMax - clipTo.drawRct.xMax ) / this.transform.lossyScale.x;
				displayRct.xMax = clipTo.drawRct.xMax;
			}
			
			if ( clipTo.drawRct.yMin > displayRct.yMin ) {
				uvRct.yMin = ( clipTo.drawRct.yMin - displayRct.yMin ) / this.transform.lossyScale.y;
				displayRct.yMin = clipTo.drawRct.yMin;
			}
			
			if ( clipTo.drawRct.yMax < displayRct.yMax ) {
				uvRct.yMax = ( displayRct.yMax - clipTo.drawRct.yMax ) / this.transform.lossyScale.y;
				displayRct.yMax = clipTo.drawRct.yMax;
			}
		}

		GL.Begin(GL.QUADS);
		
		GL.Color ( tint );
		
		material.SetPass ( 0 );

		// Bottom Left	
		GL.TexCoord2 ( uvRct.x, uvRct.y );
		GL.Vertex3 ( displayRct.x, displayRct.y, drawDepth );
		
		// Top left
		GL.TexCoord2 ( uvRct.x, uvRct.y + uvRct.height );
		GL.Vertex3 ( displayRct.x, displayRct.y + displayRct.height, drawDepth );
		
		// Top right
		GL.TexCoord2 ( uvRct.x + uvRct.width, uvRct.y + uvRct.height );
		GL.Vertex3 ( displayRct.x + displayRct.width, displayRct.y + displayRct.height, drawDepth );
		
		// Bottom right
		GL.TexCoord2 ( uvRct.x + uvRct.width, uvRct.y );
		GL.Vertex3 ( displayRct.x + displayRct.width, displayRct.y, drawDepth );
		
		GL.Color ( Color.white );

		GL.End();
	}
}
