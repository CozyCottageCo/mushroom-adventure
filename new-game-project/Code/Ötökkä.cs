
using Godot;
using System;
using SieniPeli;

namespace SieniPeli {
    public partial class Ötökkä : PathFollow2D {
        [Export] protected float maxSpeed = 100f;
        [Export] protected float kiihdytysSpeed = 40f;
        [Export] protected float jarrutusSpeed = 80f;
        [Export] protected float risteysSpeed = 50f;
        [Export] protected float minRollingSpeed = 20f;

        private CrossWalkManager CrossWalkManager;

        private float currentSpeed = 0f;
        protected bool isStopped = false;
        private bool isOnCrossWalk = false;
        private bool isOnLights = false;
        private string sieniSuojaTie = "";
        private string detectedCrossWalk = "";

        private bool isBlocked = false;
        private bool approachingRisteys = false;
        private bool inRisteys = false;

        private string blockedBy = "";
        private string blockedByFront = "";
        private bool alreadyBlockedFront = false;
        private bool alreadyBlocked = false;

        private float blockedBySpeed = 0f;

        private float stopTime = 0f;
        private const float maxStopTime = 8f;

        private Vector2 _direction;

        private Vector2 _lastDirection;
        private Vector2 _initialDirection;
        private Vector2 _initialDirectionSaved;
        private Vector2 previousPosition;
        private Path2D path = null;

        private Vector2 firstPoint;
        private Vector2 startPosition;
        private Vector2 endPosition;
        private bool isTurning = false;
        private Vector2 _turnDirection;
        private bool printed = false;

        private Area2D detectionAreaLong = null;
        private Area2D detectionAreaShort = null;
        private Area2D detectionAreaFront = null;
        private Area2D collisionArea = null;

        public bool blockedByLight = false;

        private bool waitingForNextRun= false; // kuoriaiselle vvaa omat
        private bool finishedRun= false;
        private float waitTime = 0f;
        private float waitTimer = 0f;

        public override void _Ready() {
            AddToGroup("Ötökkä"); // kaikki samaa ryhmää crosswalkmanagerin viestei varte
            CrossWalkManager = GetNode<CrossWalkManager>("/root/Node2D/CrossWalkManager");
            currentSpeed = maxSpeed;
            previousPosition = Position;

            path = GetParent<Path2D>(); // katotaan ötökän reittipath2d
            startPosition = path.Curve.GetPointPosition(0);
            firstPoint = path.Curve.GetPointPosition(1);
            endPosition = path.Curve.GetPointPosition(2);
            isTurning = IsTurning(startPosition, firstPoint, endPosition); // kääntyykö? lasketaa noist 3 pisteest
            if (isTurning) {
               // GD.Print(this.Name + "is turning");
            }
            if (isTurning) {
                //GD.Print($"{this.Name} is turning {firstPoint} {endPosition}");
               _turnDirection = SetTurnDirection(firstPoint, endPosition); // jos, minne
               //GD.Print(_turnDirection);


            }

            SetInitialDirection(firstPoint, startPosition); // alkusuunta
            _direction = _initialDirection;
            _initialDirectionSaved = _initialDirection; // tallennetaan käyttöö varte

             //GD.Print($"{this.Name} Initial Direction: ", _initialDirectionSaved);

            detectionAreaLong = GetNode<Area2D>("DetectionArea2DLong");
            detectionAreaShort = GetNode<Area2D>("DetectionArea2DShort");
            detectionAreaFront = GetNode<Area2D>("DetectionArea2DFront");
            collisionArea = GetNode<Area2D>("CollisionArea2D");
            // Signaalit kollisioiden kattomisee

            detectionAreaLong.AreaEntered += OnLongRangeEntered;
            detectionAreaLong.AreaExited += OnLongRangeExited;
            detectionAreaLong.BodyEntered += OnLongRangeEntered;
            detectionAreaLong.BodyExited += OnLongRangeExited;
            detectionAreaShort.AreaEntered += OnShortRangeEntered;
            detectionAreaShort.AreaExited += OnShortRangeExited;
            detectionAreaShort.BodyEntered += OnShortRangeEntered;
            detectionAreaShort.BodyExited += OnFrontRangeExited;
             detectionAreaFront.AreaEntered += OnFrontRangeEntered;
            detectionAreaFront.AreaExited += OnFrontRangeExited;
            collisionArea.AreaEntered += OnCrossWalkEntered;
            collisionArea.AreaExited += OnCrossWalkExited;
            collisionArea.BodyEntered += OnCrossWalkEntered;
            collisionArea.BodyExited += OnCrossWalkExited;


        }

        public override void _Process(double delta) {
            if (waitingForNextRun && finishedRun) { // tää o vaa poliisikuoriaisen timer millo lähtee uusiks
             waitTimer += (float)delta;
                if (waitTimer >= waitTime) {
                    waitingForNextRun= false;
                    waitTimer = 0f;
                    waitTime = 0f;
                    finishedRun = false;
                    ProgressRatio = 0;
                }
                return;
            }

        if (this.Name == "Kuoriainen" && Mathf.Abs(ProgressRatio) > 0.95f && !finishedRun) {
            waitingForNextRun = true;
            finishedRun = true;
              waitTime = (float)GD.RandRange(10f, 20f);
            GD.Print(waitingForNextRun);
        }


            if (blockedByFront != "") {
           // GD.Print($"{this.Name} is blockedby {blockedByFront}");
            }
            if (Mathf.Abs(ProgressRatio) > 0.95f) { // tarkistetaa et nollaa lopus
                blockedBy = "";
                }

         if (MathF.Abs(ProgressRatio) > 0.001f) {
            _direction = _initialDirectionSaved; // mm tällä varmistettii et aluks on oikee direction jos jtn muute kusi
        }
        if (MathF.Abs(ProgressRatio) > 0.01f) { // sit normaali direction check nykyse ja aiemma position eron perusteel

            _direction = SetDirection(Position, previousPosition);

        }
        if (Mathf.Abs(ProgressRatio) > 0.1f) {
        var frontRange = GetNode<Area2D>("DetectionArea2DFront");
            foreach (Area2D overlapping in frontRange.GetOverlappingAreas())
            {
                OnFrontRangeEntered(overlapping); // tän idea oli yrittää estää ettei edes oleva nollaudu ku joku muu tulee alueel kans
            }
        }

         previousPosition = Position;

        if (GetDirectionAsString(_direction) != "Unknown" && _lastDirection != _direction) {
            _lastDirection = _direction; // tää esti sen et jos ötökkä ei liiku, suunta oli 0,0: unknown. Nyt tallennetaa aiempi tiedetty suunta ja käytetää sitä animaatioo
         PlayAnimation(GetDirectionAsString(_lastDirection), currentSpeed);
        }
            if (isStopped || blockedBy != "" || blockedByFront != "") {
                stopTime += (float)delta;

                // Tarkistus et lähtevät liikkeel jos liia kaua paikallaa syystä X
                if (stopTime >= maxStopTime) {
                    GD.Print($"{this.Name} has been stopped for 10 seconds, resuming.");
                    isStopped = false;
                    blockedBy = "";
                    blockedByFront = "";
                    Resume();
                }
                return;
            }
            stopTime = 0f;
            Move((float)delta); // itse liike lopuks



        }

        protected virtual void Move(float delta) {
            if (Mathf.Abs(ProgressRatio) < 0.1f) {
                blockedByFront = "";
            }
            if (blockedByFront != "") { // elä liiku jos blocked
                return;
            }
            if (approachingRisteys) { // vanha koodi joka ei taida olla käytössä
                currentSpeed =  Mathf.MoveToward(currentSpeed, risteysSpeed, jarrutusSpeed * delta);
            } else if (inRisteys) {
                currentSpeed = risteysSpeed;
            }
            else if (!isBlocked) {
                currentSpeed = Mathf.MoveToward(currentSpeed, maxSpeed, kiihdytysSpeed * delta);
            } else {
                currentSpeed = Mathf.MoveToward(currentSpeed, minRollingSpeed, jarrutusSpeed * delta);
            }
            if (isOnLights) { // tää estää ettei pysähdy valoristeykseen suojatien päälle
                currentSpeed = Mathf.MoveToward(currentSpeed, maxSpeed, kiihdytysSpeed * delta);
            }
            Progress += currentSpeed * delta; // ja eteneminen
        }

        public virtual void Stop() {
            if (this.Name == "Kuoriainen") { // poliisi ei pysähy
                return;
            }
            if (isOnCrossWalk || isOnLights) { // eikä pysähy (pitäis) jos on suojatiel
                return;
            }
            isStopped = true;
            currentSpeed = 0f;
            stopTime = 0f;
        }

        public string GetBlocked() { // mikä blokkaa (käytetää muual)
            return blockedByFront;
        }

        public virtual void Resume() { // takas liikkeelle
            if (!isStopped || blockedByLight) return; // Return heti jos ei oo lupa liikkua

           // GD.Print($"{this.Name} resumes moving.");
            isStopped = false;
            isBlocked = false; // forcetaa viel falseks

            // kiihytys
            if (currentSpeed < maxSpeed) {
                currentSpeed = Mathf.Max(currentSpeed, kiihdytysSpeed);
            }
        stopTime = 0f;
        }

        private void PlayAnimation(string direction, float speed) { // animaatiot täält
            AnimatedSprite2D sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            sprite.FlipH = false; // nollataa kaikki aina ku playanimatio called ettei jää vanha joku päälle
            sprite.FlipV = false;
            sprite.RotationDegrees = 0;
            string targetAnimation = "";
               this.Scale = Vector2.One;

if (this.Name == "WRONG") { // tää oli debuggia varte lol
                        GD.Print(direction);
                    }
           // GD.Print($"PlayAnimation: {direction}, Speed: {speed}, CurrentAnim: {sprite.Animation}, {blockedByLight}");

            if (blockedByLight || currentSpeed == 0) // jos valois tai pysähtyny muute (ei jostai syyst ollu aina 0 speed valois?)
                {

                    // suunnan mukaan (joka on siis viimene tiedetty suunta)
                    switch (direction)
                    {
                        case "Right":
                            targetAnimation = "Stopright";
                            sprite.FlipH = true;
                            if (this.Name == "Kuoriainen") { // flippejä vähä sen mukaa mite toimis
                                sprite.FlipH = false;
                            }
                            break;
                        case "Left":
                            targetAnimation = "Stopleft";
                            sprite.FlipH = true;
                            this.Scale = new Vector2(1,-1); // täski piti flippaa ylösalasin et toimi
                            break;
                        case "Down":
                           targetAnimation = "Stopdown";
                           sprite.FlipV = true;
                            sprite.RotationDegrees = 90;
                            break;
                        case "Up":
                            targetAnimation = "Stopup";
                            sprite.RotationDegrees = -90;
                            break;
                    }
                }
                else if (!blockedByLight && currentSpeed > 0)
                {
                   // jos liikkuu ni erit toki
                    switch (direction)
                    {
                        case "Right":
                            targetAnimation = "Walkright";
                            sprite.FlipH = true;
                             if (this.Name == "Kuoriainen") {
                                sprite.FlipH = false;
                            }
                            break;
                        case "Left":
                           targetAnimation = "Walkleft";
                            sprite.FlipH = true;
                            if (this.Name == "Kuoriainen") {
                                sprite.FlipH = false;
                            }
                           this.Scale = new Vector2(1,-1); // flip koko homma et collisionshapet siirtyy kans
                            break;
                        case "Down":
                           targetAnimation = "Walkdown";
                            sprite.FlipV = true;
                            sprite.RotationDegrees = 90;
                            break;
                        case "Up":
                            targetAnimation = "Walkup";
                            sprite.RotationDegrees = -90;
                            break;
                    }
                }
                 if (sprite.Animation != targetAnimation) // ja jos haluttu animaatio ei oo se mikä päällä, pakotetaa vanha pois päältä
                {
                    sprite.Stop();
                    sprite.Play(targetAnimation);
                }
            }

        private void OnLongRangeEntered(Node body) { // kaukokollisio
            if (this.Name == "Kuoriainen") { // varmistus ettei kuoriaine tee mitn
                return;
            }
            string areaName = body.Name;
            if (body is Area2D area)
            {
                if (areaName == "Risteys" && isTurning) { // vanha koodi; ei oo enää eriksee risteyksiä tasois merkattu
                    approachingRisteys = true;
                }
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) { // jos ötökkä (eikä oo tää)
                    var otherÖtökkä = parentNode as Ötökkä; // laitetaa muistii, katotaa sen tietoi jne
                    if (otherÖtökkä != null) {
                        Vector2 other_direction = otherÖtökkä.GetDirection();
                        bool otherTurning = otherÖtökkä.GetTurning();
                        bool otherTurned = otherÖtökkä.HasTurned();
                        bool otherRisteys = false;
                        if (otherTurning) {
                            otherRisteys = otherÖtökkä.GetRisteys();
                            if(!otherRisteys) {
                            other_direction = otherÖtökkä.GetTurnDirection();
                            }
                        }   // tietojen perusteella katotaa pitäskö sitä väistää vai ei
                        if (ShouldYieldWithTurning(_direction, other_direction, isTurning, otherTurning, otherRisteys, otherÖtökkä.Name, otherTurned)) {
                            isBlocked = true;
                            blockedBySpeed = otherÖtökkä.GetSpeed(); // jos pitää, blokkia
                        }
                    }
                }
            }
            else if (body is TileMapLayer tileMapLayer)
            {
                if (areaName.StartsWith("Suojatie")) // jos näkee suojatien ja siel on sieni, alkaa hidastaa valmiiks
                {

                        detectedCrossWalk = areaName;
                        if(CrossWalkManager.CrossWalkOccupied &&
                        CrossWalkManager.CurrentCrossWalk == detectedCrossWalk) {
                            isBlocked = true;
                        }
                }

            }

        }

        private void OnLongRangeExited(Node body) { // ja jutut pois ku lähtee alueelt
            if (this.Name == "Kuoriainen") {
                return;
            }
            string areaName = body.Name;
            if (body is Area2D area) {

                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {


                    isBlocked = false;
                }
            }
            else if (body is TileMapLayer tileMapLayer)
                {
                    if (areaName.StartsWith("Suojatie"))
                    {
                        isBlocked = false;
                        detectedCrossWalk = "";
                        CheckCrossWalkStatus();
                    }
                }
        }

        private void OnShortRangeEntered(Node body) { // ja lyhyemmän rangen kollisiotsek
            if (this.Name == "Kuoriainen") {
                return;
            }
            string areaName = body.Name;
            if (body is Area2D area)
            {

                if (blockedBy != "") {
                if (body.GetInstanceId().ToString() != blockedBy || alreadyBlocked) {
                  //  mm täs koitettii estää ettei se vaihda ketä blokkaa jos toinenki tulee alueelle
                return;
                }
            }


                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    var otherÖtökkä = parentNode as Ötökkä; // ja taas ötökän tietoja taltee
                    if (otherÖtökkä != null) {
                        Vector2 other_direction = otherÖtökkä.GetDirection();
                        bool otherTurning = otherÖtökkä.GetTurning();
                        bool otherTurned = otherÖtökkä.HasTurned();
                        bool otherRisteys = false;
                        if (otherTurning) {
                            otherRisteys = otherÖtökkä.GetRisteys();
                            if(!otherRisteys) {
                            other_direction = otherÖtökkä.GetTurnDirection();
                            }
                        }
                        if (ShouldYieldWithTurning(_direction, other_direction, isTurning, otherTurning, otherRisteys, otherÖtökkä.Name, otherTurned)) {
                           // ja katotaa pitäskö väistää
                            if (String.IsNullOrEmpty(blockedBy)) {
                          // jos EI oo blokattu jo ni otetaa tää siihe
                            blockedBy = body.GetInstanceId().ToString();
                            alreadyBlocked = true;
                            }
                            Stop();
                           // ja stoppia
                        }
                    }

                    CheckCrossWalkStatus(); // näit tarkastuksii aina välil
                }
            }
            else if (body is TileMapLayer tileMapLayer)
            {
                if (areaName.StartsWith("Suojatie")) // suojatie kans tsek pitääkö pysähtyä
                {
                        detectedCrossWalk = areaName;
                        if(CrossWalkManager.CrossWalkOccupied &&
                        CrossWalkManager.CurrentCrossWalk == detectedCrossWalk) {
                            Stop();
                            CheckCrossWalkStatus();
                        }
                }

            }
        }

        private void OnShortRangeExited(Node body) { // ja ulos päinvastoi
            if (this.Name == "Kuoriainen") {
                return;
            }
            string areaName = body.Name;


            if (body is Area2D area) {

                 if (blockedBy != "") {
                if (body.GetInstanceId().ToString() != blockedBy) {
                return;
                }

            }
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    if (body.GetInstanceId().ToString() == blockedBy) {
                    blockedBy = "";
                    alreadyBlocked = false;
                    Resume();
                    }
                }
            }
            else if (body is TileMapLayer tileMapLayer)
                {
                    if (areaName.StartsWith("Suojatie"))
                    {
                        Resume();
                        detectedCrossWalk = "";
                        CheckCrossWalkStatus();
                    }
                }
        }

        private void OnFrontRangeEntered(Node body) { // iha edes olevien tsekki (ettei aja päälle)
            if (this.Name == "Kuoriainen") {
                return;
            }
            string areaName = body.Name;
            if (body is Area2D area)
            {
                if (areaName == "Valotie") {
                    blockedByLight = true; //valoteil oma ku siin on area2d
                    Stop();
                }
                else if (blockedByFront != "") {
                if (body.GetInstanceId().ToString() != blockedByFront || alreadyBlockedFront) {
                return;
                }
            }
                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {
                    var otherÖtökkä = parentNode as Ötökkä;
                    if (otherÖtökkä != null) {
                        Vector2 other_direction = otherÖtökkä.GetDirection();
                        string otherBlocked = otherÖtökkä.GetBlocked();
                        if (GetDirectionAsString(_direction) == GetDirectionAsString(other_direction)){

                        if (otherBlocked == this.GetInstanceId().ToString()) { // verrataan onko toisen ötökän blokin id tämän id -> etteivät blokkaa toisiaan ja jumita
                            GD.Print("other bug already blocked by this, reverting");
                            blockedByFront = "";
                            alreadyBlockedFront = false;
                            return;
                        }
                        blockedByFront = parentNode.GetInstanceId().ToString();
                        alreadyBlockedFront = true;
                            }
                        }
                    }
            }
        }

         private void OnFrontRangeExited(Node body) {
            if (this.Name == "Kuoriainen") {
                return;
            }
            string areaName = body.Name;
            if (body is Area2D area) {

                if (areaName == "Valotie" && body != this) {
                    GD.Print("Set to false here");
                    blockedByLight = false;
                    Resume();

                }

                if (body.GetParent() is Node parentNode && parentNode.IsInGroup("Ötökkä") && parentNode != this) {

                    if (parentNode.GetInstanceId().ToString() == blockedByFront) {

                    blockedByFront = "";
                    alreadyBlockedFront = false;
                    Resume();
                    }
                }
            }
         }

         private void OnCrossWalkEntered(Node body) { // katotaa ollaanko suojatien päällä

            string areaName = body.Name;

            if (body is TileMapLayer tileMapLayer)
            {

                 if (!isOnCrossWalk)
                {
                    isOnCrossWalk = true;
                    Stop(); // katotaa ettei pysähytä (siel heti alus return jos on nääs)
                }

            }
            if (areaName == "Valotie") {
                isOnLights = true;
                blockedByLight = false;
                Resume();

            }
            if (areaName == "Risteys" && isTurning) { // vanhaa koodia tosiaan
                approachingRisteys = false;
                inRisteys = true;
            } else {
                approachingRisteys = false;
            }
        }
        private void OnCrossWalkExited(Node Area2D) {
            string areaName = Area2D.Name;

            if (isOnCrossWalk) {
                isOnCrossWalk = false;
                CheckCrossWalkStatus();
            }
            if (areaName == "Risteys") {
                inRisteys = false;
            }
            if (areaName == "Valotie") {
                isOnLights = false;
            }
        }

        private void CheckCrossWalkStatus()
        {
            sieniSuojaTie = CrossWalkManager.CurrentCrossWalk; // katotaa crosswalkmanagerista tosiaa pitääkö suojatietä varte pysähtyä

           if (!string.IsNullOrEmpty(detectedCrossWalk) && CrossWalkManager.CrossWalkOccupied &&
            sieniSuojaTie == detectedCrossWalk && OnHeijastin()) // heijastintsekki myös jos relevantti
            {
                Stop();
            }
            else
            {
                Resume();
            }
            }


            public Vector2 GetDirection() {
                return _direction;
            }
            public bool GetTurning() {
                return isTurning;
            }

            public float GetSpeed() {
                return currentSpeed;
            }

            public bool GetRisteys() {
                return inRisteys;
            }
            private bool IsSame_direction(Vector2 other_direction) { // täs verrattii mennäänkö samaa suuntaa, taitaa olla vanhaa koodia
                float dotProduct = _direction.Dot(other_direction);
                return dotProduct > 0f;
            }

            public bool OnHeijastin() { // tarkistus, simppelisti levelin mukaan
            string scenePath = GetTree().CurrentScene.SceneFilePath;
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Sieni sieni = GetNode<Sieni>("/root/Node2D/Sieni");
            if (sceneName == "Level14" || sceneName == "Level15" || sceneName == "Level16" || sceneName == "Levelhuussi") { // insert leveli(t) missä heijastinta käytetään
                if (sieni._hasHeijastin) {
                    return true;
                } else {
                    return false; // ainoostaan false jos heijastinkartta ja ei oo mukana
                }
            }
            return true; // true muihin ku heijastinkarttoihin
            }

            private bool IsOppositeDirection(Vector2 otherBugDirection) { // onko vastakkainen suunta toisen ötökän kanssa
                string thisDirectionString = GetDirectionAsString(_direction);
               string otherBugDirectionString = GetDirectionAsString(otherBugDirection);

                switch (thisDirectionString, otherBugDirectionString) {
                    case ("Up", "Down"):
                    return true;
                    case ("Down", "Up"):
                    return true;
                    case ("Left", "Right"):
                    return true;
                    case ("Right", "Left"):
                    return true;
                    default:
                    return false;
                }
            }


private Vector2 GetCardinalDirection(Vector2 direction) { // metodi auttaa koordinaattien siistimises
    const float THRESHOLD = 0.01f;  // pienet erot = 0

    if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y) + THRESHOLD) {
        return new Vector2(Mathf.Sign(direction.X), 0);  // vasen oikee
    }
    else if (Mathf.Abs(direction.Y) > Mathf.Abs(direction.X) + THRESHOLD) {
        return new Vector2(0, Mathf.Sign(direction.Y));  // ylös alas
    }
    else {
        return new Vector2(0, 0);  // Unknown jos ei liikettä tarpeeks
    }
}



private bool IsTurning(Vector2 start, Vector2 middle, Vector2 end) { // tarkistus kääntyykö tämä ötökkä
    Vector2 dir1 = (middle - start).Normalized();
    Vector2 dir2 = (end - middle).Normalized();

    float angleChange = dir1.AngleTo(dir2);

    const float TURN_ANGLE_THRESHOLD = 0.3f; // ~17 asteen kulma riittää

    return Mathf.Abs(angleChange) > TURN_ANGLE_THRESHOLD;
}


private void SetInitialDirection (Vector2 firstPoint, Vector2 startPosition) { // metodi jossa laitetaa alotussuunta (talteen)

                _initialDirection = (firstPoint - startPosition).Normalized();
                 const float THRESHOLD = 0.1f;
            if (Mathf.Abs(_initialDirection.Y) < THRESHOLD) {
                _initialDirection.Y = 0f; // liia pieni muutos o 0
            }
            else if (_initialDirection.Y > THRESHOLD) {
                _initialDirection.Y = 1f; // positiivinen y
            }
            else if (_initialDirection.Y < -THRESHOLD) {
                _initialDirection.Y = -1f; // Negatiivinen y
            }

            if (Mathf.Abs(_initialDirection.X) < THRESHOLD) {
                _initialDirection.X = 0f; // liia pieni = 0
            }
            else if (_initialDirection.X > THRESHOLD) {
                _initialDirection.X = 1f; // positiivinen x muutos
            }
            else if (_initialDirection.X < -THRESHOLD) {
                _initialDirection.X = -1f; // negatiivinen x muutos
            }
                        _direction = _initialDirection;
            }

private Vector2 SetDirection (Vector2 position, Vector2 lastposition) { // asetetaan suunta
                Vector2 direction = (position - lastposition); // positioiden vertailun perusteella
                Vector2 cardinalDirection = GetCardinalDirection(direction);


               if (cardinalDirection == Vector2.Zero || (Mathf.Abs(cardinalDirection.X) > 0 && Mathf.Abs(cardinalDirection.Y) > 0)) {
                    cardinalDirection = _lastDirection; // jos tulee 0,0 tjsp "unknown" niin käytetää vanhaa tunnettua suuntaa
               }
           return cardinalDirection;
}



    private string GetDirectionAsString(Vector2 direction) { // lukemise helpotuksee suunnat tekstinä

    if (direction == new Vector2(1,0)) return "Right";
    if (direction == new Vector2(-1,0)) return "Left";
    if (direction == new Vector2(0,1)) return "Down";
    if (direction == new Vector2(0,-1)) return "Up";


    return "Unknown";
}

private Vector2 SetTurnDirection(Vector2 point1, Vector2 point2) // liikennelogiikkaa varte kääntymisen suunnan asetus(muistiin)
{
    const float THRESHOLD = 0.1f;

    // Calculate the direction vector based on the difference between the points
    Vector2 direction = (point2 - point1).Normalized();

    // Apply the threshold logic to determine the direction
    if (Mathf.Abs(direction.Y) < THRESHOLD) {
        direction.Y = 0f; // Small Y changes, treat as 0
    }
    else if (direction.Y > THRESHOLD) {
        direction.Y = 1f; // Positive Y, treat as down (1)
    }
    else if (direction.Y < -THRESHOLD) {
        direction.Y = -1f; // Negative Y, treat as up (-1)
    }

    // Handle X direction
    if (Mathf.Abs(direction.X) < THRESHOLD) {
        direction.X = 0f; // Small X changes, treat as 0
    }
    else if (direction.X > THRESHOLD) {
        direction.X = 1f; // Positive X, treat as right (1)
    }
    else if (direction.X < -THRESHOLD) {
        direction.X = -1f; // Negative X, treat as left (-1)
    }

    // Return the final direction as a Vector2
    return direction;
}


public Vector2 GetTurnDirection() {
    return _turnDirection;
}

private bool RightTurn() {
    string turnDirectionString = GetDirectionAsString(_turnDirection);
    string currentDirectionString = GetDirectionAsString(_initialDirectionSaved);
   // GD.Print(this.Name, currentDirectionString, turnDirectionString);
    switch (currentDirectionString, turnDirectionString){
        case ("Up", "Right"):
        return true;
        case ("Left", "Up"):
        return true;
        case ("Down", "Left"):
        return true;
        case ("Right", "Down"):
        return true;
        default:
        return false;
    }
}

public bool HasTurned() {
    string initialDirection = GetDirectionAsString(_initialDirectionSaved);
    string currentDirection = GetDirectionAsString(_direction);

    if (initialDirection != currentDirection) {
        return true;
    }
    else {
        return false;
    }

}


private bool ShouldYieldWithTurning(Vector2 thisDirection, Vector2 otherDirection, bool isTurning, bool otherTurning, bool otherRisteys, string otherÖtökkä, bool otherTurned)
{
    string thisBugDirection = GetDirectionAsString(thisDirection);
    string otherBugDirection = GetDirectionAsString(otherDirection);
    if (thisBugDirection == "Unknown") {
      //  GD.Print(thisDirection);
        if(!isTurning) {
        thisBugDirection = GetDirectionAsString(_initialDirectionSaved);
        }
        else {
            thisBugDirection = GetDirectionAsString(_turnDirection);
        }
    }

    //GD.Print($"Checking yield: {this.Name} ({thisBugDirection}, Turning: {isTurning}), {otherÖtökkä}({otherBugDirection}, Turning: {otherTurning}, InIntersection: {otherRisteys})");

    if (isTurning) {
        thisBugDirection = GetDirectionAsString(_turnDirection);
        //GD.Print($"{this.Name} bug is turning, using turn direction: {thisBugDirection}");
    }



    // Case 1: If both cars are not turning (both are going straight), apply the straight yield rules.
    if (!isTurning && !otherTurning)
    {
       // GD.Print("Both cars are going straight, checking straight yield rules...");
        if (thisBugDirection == "Up" && otherBugDirection == "Left") {
           // GD.Print("Car going Up yields to car going Left.");
            return true;
        }
        if (thisBugDirection == "Down" && otherBugDirection == "Right") {
          //  GD.Print("Car going Down yields to car going Right.");
            return true;
        }
        if (thisBugDirection == "Left" && otherBugDirection == "Down") {
            //GD.Print("Car going Left yields to car going Down.");
            return true;
        }
        if (thisBugDirection == "Right" && otherBugDirection == "Up") {
           // GD.Print("Car going Right yields to car going Up.");
            return true;
        }
    }

    // Case 2: If the current car is turning and the other car is not, the current car yields.
    if (isTurning && !otherTurning && !IsOppositeDirection(otherDirection))
    {
        if(RightTurn()){
            return false;
        } else {
       // GD.Print("This bug is turning while the other bug is not. Yielding...");
        return true;
        }

    }

  // Case 3: If the other car is turning and the current car is not, the other car yields.
    if (!isTurning && otherTurning)
    {

       //GD.Print($"Other bug is turning while {this.Name} bug is not.");
        if (otherRisteys && !IsOppositeDirection(otherDirection)) {
            //GD.Print(thisBugDirection, otherBugDirection); // tää ei toimiiiii. Pitäs kattoo initial direction tjsp.
            return true;
        }
       // GD.Print("Other bug is turning but not in front. No yield needed.");
        return false;
    }

    // Case 4: Both cars are turning
    if (isTurning && otherTurning)
    {

       // GD.Print("Both bugs are turning, checking crossing conditions...");
        if (thisBugDirection == "Right" && otherBugDirection == "Right")
        {
          //  GD.Print("Both bugs are turning right, no yield needed.");
            return false;
        }
        if (thisBugDirection == "Left" && otherBugDirection == "Left")
        {
            //GD.Print("Both bugs are turning left, they yield to each other.");
            return true;
        }
        if ((thisBugDirection == "Left" && otherBugDirection == "Right") ||
            (thisBugDirection == "Right" && otherBugDirection == "Left"))
        {
            //GD.Print("One bug is turning left and the other is turning right, they yield to each other.");
            return true;
        }
    }

   // GD.Print("No specific yield condition met. No yield needed.");
    return false;
}
    }
}