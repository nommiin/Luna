using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Luna.Runner {
    class View {
        public double X;
        public double Y;
        public double W;
        public double H;
        public bool Visible;
        //todo: Most view values should be handled by cameras once that's implemented

        public void Update() {

        }

        public void ApplyTransform() {
            Matrix4 _transform = Matrix4.Identity;
        }
    }

    
}
