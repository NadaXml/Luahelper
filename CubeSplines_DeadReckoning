            local CubeSplines_DeadReckoning = {}

            function CubeSplines_DeadReckoning.Init( pos, dstPos , inputVec , inputA , dstVec, dstA, t )

                local p1 = pos 
                local p2 = pos + inputVec + inputA * t
                local p4 = dstPos + dstVec * t + dstA * 0.5 * t * t
                local p3 = p4 - (dstVec * t + dstA * t) 
                
                local this = CubeSplines_DeadReckoning
                this.A = p4.x - 3 * p3.x + 3 * p2.x - p1.x
                this.B = 3 * p3.x - 6  * p2.x + 3* p1.x
                this.C = 3 * p2.x - 3 * p1.x
                this.D = p1.x

                this.E = p4.z - 3*p3.z + 3*p2.z - p1.z
                this.F = 3*p3.z - 6*p2.z + 3*p1.z
                this.G = 3*p2.z - 3 *p1.z
                this.H = p1.z

            end

            function CubeSplines_DeadReckoning.Calcute(t , h)
                local this = CubeSplines_DeadReckoning
                local dd = Vector3.New(this.A*t*t*t + this.B*t*t + this.C*t + this.D, h, this.E*t*t*t + this.F*t*t + this.G*t + this.H)
                return dd
            end

            local tt = 1
            local a = Vector3.New(0,0,0)

            local v = Vector3.New(3,0,0)
            local v2 = Vector3.New(3,0,3)

            local orign = LocalPlayer.main.pos  
            local tar = LocalPlayer.main.pos + Vector3(5,0,5)

            local h = orign.y

            CubeSplines_DeadReckoning.Init(orign, tar , v, a, v2, a, tt)

            local t = 0

            for i=1, 30 do 
                local dd = CubeSplines_DeadReckoning.Calcute(t,h)
                Log.Red(string.format( "x:%s,y:%s，z:%s",dd.x,dd.y,dd.z))
                t = i / 30 
            end
            
