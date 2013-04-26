using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HLSLTest
{
	public struct ParticleVertex : IVertexType
	{
		Vector3 startPosition;
		Vector2 uv;
		Vector3 direction;
		float rotation;
		float speed;
		float startTime;

		// Starting position of that particle (t = 0)
		public Vector3 StartPosition
		{
			get { return startPosition; }
			set { startPosition = value; }
		}
		// UV coordinate, used for texturing and to offset vertex in shader
		public Vector2 UV
		{
			get { return uv; }
			set { uv = value; }
		}
		// Movement direction of the particle
		public Vector3 Direction
		{
			get { return direction; }
			set { direction = value; }
		}
		public float Rotation
		{
			get { return rotation; }
			set { rotation = value; }
		}
		// Speed of the particle in units/second
		public float Speed
		{
			get { return speed; }
			set { speed = value; }
		}
		// The time since the particle system was created that this
		// particle came into use
		public float StartTime
		{
			get { return startTime; }
			set { startTime = value; }
		}
		

		// Vertex declaration
		public readonly static VertexDeclaration VertexDeclaration =
			new VertexDeclaration(
				/*// Start position
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				// UV coordinates
				new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
				// Movement direction
				new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1),
				// Movement speed
				new VertexElement(32, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
				// Start time
				new VertexElement(36, VertexElementFormat.Single,VertexElementUsage.TextureCoordinate, 3)*/
				// Start position
					new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				// UV coordinates
					new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
				// Movement direction
					new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1),
				// Rotation
					new VertexElement(32, VertexElementFormat.Single, VertexElementUsage.Color, 0),
				// Movement speed
					new VertexElement(36, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
				// Start time
					new VertexElement(40, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3)
			);
		VertexDeclaration IVertexType.VertexDeclaration
		{
			get { return VertexDeclaration; }
		}


		// Constructor
		public ParticleVertex(Vector3 StartPosition, Vector2 UV, Vector3 Direction, float Speed, float StartTime)
			:this(StartPosition, UV, Direction, 0, Speed, StartTime)
		{
			this.startPosition = StartPosition;
			this.uv = UV;
			this.direction = Direction;
			this.speed = Speed;
			this.startTime = StartTime;
		}/**/
		public ParticleVertex(Vector3 StartPosition, Vector2 UV, Vector3 Direction, float rotation, float Speed, float StartTime)
		{
			this.startPosition = StartPosition;
			this.uv = UV;
			this.direction = Direction;
			this.rotation = rotation;
			this.speed = Speed;
			this.startTime = StartTime;
		}
	}
}
