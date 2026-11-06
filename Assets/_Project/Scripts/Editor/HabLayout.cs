namespace GameOff.EditorTools
{
    /// <summary>
    /// Floor plan of the walkable hab module (P53), generated once from the pipeline's docs/station/wohnmodul_layout.json.
    /// World coordinates in meters, yaw around +Y. Door walls of the hab doors are in Inside, the door leaves come from Doors.
    /// </summary>
    public static class HabLayout
    {
        public readonly struct Piece
        {
            public readonly string Kit;
            public readonly float X, Y, Z, Yaw;
            public Piece(string kit, float x, float y, float z, float yaw) { Kit = kit; X = x; Y = y; Z = z; Yaw = yaw; }
        }

        public readonly struct Door
        {
            public readonly string Name;
            public readonly bool OnXLine;
            public readonly float Line, Center, Width;
            public readonly GameOff.Station.DoorAccess Access;
            public Door(string name, bool onXLine, float line, float center, float width, GameOff.Station.DoorAccess access)
            { Name = name; OnXLine = onXLine; Line = line; Center = center; Width = width; Access = access; }
        }

        public readonly struct RoomArea
        {
            public readonly int Number;
            public readonly float X0, X1, Z0, Z1;
            public RoomArea(int number, float x0, float x1, float z0, float z1) { Number = number; X0 = x0; X1 = x1; Z0 = z0; Z1 = z1; }
        }

        public const float DeckY = 7.42f;

        public static readonly Piece[] Inside =
        {
            new Piece("Station/SM_Station_HabTube_Frame.fbx", -20f, 0f, 1f, -90f),
            new Piece("Station/SM_Station_HabTube_Stair_2m.fbx", -22.5f, 0f, 1f, -90f),
            new Piece("Station/SM_Station_HabTube_Stair_2m.fbx", -24.5f, 1.237f, 1f, -90f),
            new Piece("Station/SM_Station_HabTube_Stair_2m.fbx", -26.5f, 2.473f, 1f, -90f),
            new Piece("Station/SM_Station_HabTube_Stair_2m.fbx", -28.5f, 3.71f, 1f, -90f),
            new Piece("Station/SM_Station_HabTube_Stair_2m.fbx", -30.5f, 4.947f, 1f, -90f),
            new Piece("Station/SM_Station_HabTube_Stair_2m.fbx", -32.5f, 6.183f, 1f, -90f),
            new Piece("Station/SM_Station_HabTube_Corner.fbx", -34.5f, 7.42f, 1f, -90f),
            new Piece("Station/SM_Station_HabTube_Straight_2m.fbx", -36.5f, 7.42f, 3f, 0f),
            new Piece("Station/SM_Station_HabTube_Straight_2m.fbx", -36.5f, 7.42f, 5f, 0f),
            new Piece("Station/SM_Station_HabTube_Straight_2m.fbx", -36.5f, 7.42f, 7f, 0f),
            new Piece("Station/SM_Station_HabTube_Straight_2m.fbx", -36.5f, 7.42f, 9f, 0f),
            new Piece("Station/SM_Station_HabTube_Collar.fbx", -36.5f, 7.42f, 11f, 0f),
            new Piece("Station/SM_Station_Hab_Bulkhead.fbx", -36.5f, 7.42f, 14f, 0f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -36.5f, 7.42f, 14f, 180f),
            new Piece("Station/SM_Station_Hab_Bulkhead.fbx", -36.5f, 7.42f, 38f, 0f),
            new Piece("Station/SM_Station_Wall_2x3_Screen.fbx", -36.5f, 7.42f, 38f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 15f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 15f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 15f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 15f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 15f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 17f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 17f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 17f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 17f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 17f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 19f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 19f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 19f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 19f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 19f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 21f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 21f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 21f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 21f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 21f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 23f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 23f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 23f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 23f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 23f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 25f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 25f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 25f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 25f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 25f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 27f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 27f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 27f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 27f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 27f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 29f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 29f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 29f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 29f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 29f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 31f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 31f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 31f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 31f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 31f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 33f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 33f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 33f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 33f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 33f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 35f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 35f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 35f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 35f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 35f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -40.5f, 7.32f, 37f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -38.5f, 7.32f, 37f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -36.5f, 7.32f, 37f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -34.5f, 7.32f, 37f, 0f),
            new Piece("Station/SM_Station_Floor_2x2.fbx", -32.5f, 7.32f, 37f, 0f),
            new Piece("Station/SM_Station_Hab_Shell_4m.fbx", -36.5f, 7.42f, 16f, 0f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -35.4f, 7.42f, 15f, 90f),
            new Piece("Station/SM_Station_Wall_2x3_Panel.fbx", -35.4f, 7.42f, 17f, 90f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -37.6f, 7.42f, 15f, -90f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -37.6f, 7.42f, 17f, -90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 15f, 90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 17f, 90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -33.5f, 10.42f, 16f, 0f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -39.5f, 10.42f, 16f, 0f),
            new Piece("Station/SM_Station_Hab_Shell_4m.fbx", -36.5f, 7.42f, 20f, 0f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -35.4f, 7.42f, 19f, 90f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -35.4f, 7.42f, 21f, 90f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -37.6f, 7.42f, 19f, -90f),
            new Piece("Station/SM_Station_Wall_2x3_Padded.fbx", -37.6f, 7.42f, 21f, -90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 19f, 90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 21f, 90f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -38.7f, 7.42f, 18f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -40.7f, 7.42f, 18f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -34.3f, 7.42f, 18f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -32.3f, 7.42f, 18f, 0f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -33.5f, 10.42f, 20f, 0f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -39.5f, 10.42f, 20f, 0f),
            new Piece("Station/SM_Station_Hab_Shell_4m.fbx", -36.5f, 7.42f, 24f, 0f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -35.4f, 7.42f, 23f, 90f),
            new Piece("Station/SM_Station_Wall_2x3_Padded.fbx", -35.4f, 7.42f, 25f, 90f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -37.6f, 7.42f, 23f, -90f),
            new Piece("Station/SM_Station_Wall_2x3_Panel.fbx", -37.6f, 7.42f, 25f, -90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 23f, 90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 25f, 90f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -38.7f, 7.42f, 22f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -40.7f, 7.42f, 22f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -34.3f, 7.42f, 22f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -32.3f, 7.42f, 22f, 0f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -33.5f, 10.42f, 24f, 0f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -39.5f, 10.42f, 24f, 0f),
            new Piece("Station/SM_Station_Hab_Shell_4m.fbx", -36.5f, 7.42f, 28f, 0f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -35.4f, 7.42f, 27f, 90f),
            new Piece("Station/SM_Station_Wall_2x3_Panel.fbx", -35.4f, 7.42f, 29f, 90f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -37.6f, 7.42f, 27f, -90f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -37.6f, 7.42f, 29f, -90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 27f, 90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 29f, 90f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -38.7f, 7.42f, 26f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -40.7f, 7.42f, 26f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -34.3f, 7.42f, 26f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -32.3f, 7.42f, 26f, 0f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -33.5f, 10.42f, 28f, 0f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -39.5f, 10.42f, 28f, 0f),
            new Piece("Station/SM_Station_Hab_Shell_4m.fbx", -36.5f, 7.42f, 32f, 0f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -35.4f, 7.42f, 31f, 90f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -35.4f, 7.42f, 33f, 90f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -37.6f, 7.42f, 31f, -90f),
            new Piece("Station/SM_Station_Wall_2x3_Padded.fbx", -37.6f, 7.42f, 33f, -90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 31f, 90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 33f, 90f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -38.7f, 7.42f, 30f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -40.7f, 7.42f, 30f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -34.3f, 7.42f, 30f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -32.3f, 7.42f, 30f, 0f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -33.5f, 10.42f, 32f, 0f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -39.5f, 10.42f, 32f, 0f),
            new Piece("Station/SM_Station_Hab_Shell_4m.fbx", -36.5f, 7.42f, 36f, 0f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -35.4f, 7.42f, 35f, 90f),
            new Piece("Station/SM_Station_Wall_2x3_Padded.fbx", -35.4f, 7.42f, 37f, 90f),
            new Piece("Station/SM_Station_Wall_Door_2x3_Narrow.fbx", -37.6f, 7.42f, 35f, -90f),
            new Piece("Station/SM_Station_Wall_2x3_Panel.fbx", -37.6f, 7.42f, 37f, -90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 35f, 90f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -36.5f, 11.01f, 37f, 90f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -38.7f, 7.42f, 34f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -40.7f, 7.42f, 34f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -34.3f, 7.42f, 34f, 0f),
            new Piece("Station/SM_Station_Wall_2x3.fbx", -32.3f, 7.42f, 34f, 0f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -33.5f, 10.42f, 36f, 0f),
            new Piece("Station/SM_Station_Hab_LightStrip_2m.fbx", -39.5f, 10.42f, 36f, 0f),
        };

        /// <summary>Bed, locker, table: these stop the player.</summary>
        public static readonly Piece[] Furniture =
        {
            new Piece("Motel/SM_Motel_Bed.fbx", -32.05f, 7.42f, 16.9f, 0f),
            new Piece("Motel/SM_Motel_Locker.fbx", -34.9f, 7.42f, 17.65f, 0f),
            new Piece("Motel/SM_Motel_Table.fbx", -31.9f, 7.42f, 14.4f, 180f),
            new Piece("Motel/SM_Motel_Bed.fbx", -40.95f, 7.42f, 16.9f, 0f),
            new Piece("Motel/SM_Motel_Locker.fbx", -38.1f, 7.42f, 17.65f, 0f),
            new Piece("Motel/SM_Motel_Table.fbx", -41.1f, 7.42f, 14.4f, 180f),
            new Piece("Motel/SM_Motel_Bed.fbx", -32.05f, 7.42f, 20.9f, 0f),
            new Piece("Motel/SM_Motel_Locker.fbx", -34.9f, 7.42f, 21.65f, 0f),
            new Piece("Motel/SM_Motel_Table.fbx", -31.9f, 7.42f, 18.4f, 180f),
            new Piece("Motel/SM_Motel_Bed.fbx", -40.95f, 7.42f, 20.9f, 0f),
            new Piece("Motel/SM_Motel_Locker.fbx", -38.1f, 7.42f, 21.65f, 0f),
            new Piece("Motel/SM_Motel_Table.fbx", -41.1f, 7.42f, 18.4f, 180f),
            new Piece("Motel/SM_Motel_Bed.fbx", -32.05f, 7.42f, 24.9f, 0f),
            new Piece("Motel/SM_Motel_Locker.fbx", -34.9f, 7.42f, 25.65f, 0f),
            new Piece("Motel/SM_Motel_Table.fbx", -31.9f, 7.42f, 22.4f, 180f),
            new Piece("Motel/SM_Motel_Bed.fbx", -40.95f, 7.42f, 24.9f, 0f),
            new Piece("Motel/SM_Motel_Locker.fbx", -38.1f, 7.42f, 25.65f, 0f),
            new Piece("Motel/SM_Motel_Table.fbx", -41.1f, 7.42f, 22.4f, 180f),
            new Piece("Motel/SM_Motel_Bed.fbx", -32.05f, 7.42f, 28.9f, 0f),
            new Piece("Motel/SM_Motel_Locker.fbx", -34.9f, 7.42f, 29.65f, 0f),
            new Piece("Motel/SM_Motel_Table.fbx", -31.9f, 7.42f, 26.4f, 180f),
            new Piece("Motel/SM_Motel_Bed.fbx", -40.95f, 7.42f, 28.9f, 0f),
            new Piece("Motel/SM_Motel_Locker.fbx", -38.1f, 7.42f, 29.65f, 0f),
            new Piece("Motel/SM_Motel_Table.fbx", -41.1f, 7.42f, 26.4f, 180f),
            new Piece("Motel/SM_Motel_Bed.fbx", -32.05f, 7.42f, 32.9f, 0f),
            new Piece("Motel/SM_Motel_Locker.fbx", -34.9f, 7.42f, 33.65f, 0f),
            new Piece("Motel/SM_Motel_Bed.fbx", -40.95f, 7.42f, 32.9f, 0f),
            new Piece("Motel/SM_Motel_Locker.fbx", -38.1f, 7.42f, 33.65f, 0f),
            new Piece("Motel/SM_Motel_Locker.fbx", -31.85f, 7.42f, 34.65f, 90f),
            new Piece("Motel/SM_Motel_Locker.fbx", -31.85f, 7.42f, 35.35f, 90f),
            new Piece("Motel/SM_Motel_Locker.fbx", -31.85f, 7.42f, 36.05f, 90f),
        };

        /// <summary>The pressure tube seen from outside.</summary>
        public static readonly Piece[] Outside =
        {
            new Piece("Exterior/SM_Ext_HabTube_Flange.fbx", -20f, 0f, 1f, -90f),
            new Piece("Exterior/SM_Ext_HabTube_Stair_2m.fbx", -22.5f, 0f, 1f, -90f),
            new Piece("Exterior/SM_Ext_HabTube_Stair_2m.fbx", -24.5f, 1.237f, 1f, -90f),
            new Piece("Exterior/SM_Ext_HabTube_Stair_2m.fbx", -26.5f, 2.473f, 1f, -90f),
            new Piece("Exterior/SM_Ext_HabTube_Stair_2m.fbx", -28.5f, 3.71f, 1f, -90f),
            new Piece("Exterior/SM_Ext_HabTube_Stair_2m.fbx", -30.5f, 4.947f, 1f, -90f),
            new Piece("Exterior/SM_Ext_HabTube_Stair_2m.fbx", -32.5f, 6.183f, 1f, -90f),
            new Piece("Exterior/SM_Ext_HabTube_Corner.fbx", -34.5f, 7.42f, 1f, -90f),
            new Piece("Exterior/SM_Ext_HabTube_Straight_2m.fbx", -36.5f, 7.42f, 3f, 0f),
            new Piece("Exterior/SM_Ext_HabTube_Straight_2m.fbx", -36.5f, 7.42f, 5f, 0f),
            new Piece("Exterior/SM_Ext_HabTube_Straight_2m.fbx", -36.5f, 7.42f, 7f, 0f),
            new Piece("Exterior/SM_Ext_HabTube_Straight_2m.fbx", -36.5f, 7.42f, 9f, 0f),
        };

        public static readonly Door[] Doors =
        {
            new Door("Door_HabSouth", false, 14f, -36.5f, 1.2f, GameOff.Station.DoorAccess.Public),
            new Door("Door_Hab201", true, -35.4f, 15f, 1.2f, GameOff.Station.DoorAccess.Locked),
            new Door("Door_Hab202", true, -37.6f, 15f, 1.2f, GameOff.Station.DoorAccess.Locked),
            new Door("Door_Hab203", true, -35.4f, 19f, 1.2f, GameOff.Station.DoorAccess.Locked),
            new Door("Door_Hab204", true, -37.6f, 19f, 1.2f, GameOff.Station.DoorAccess.Locked),
            new Door("Door_Hab205", true, -35.4f, 23f, 1.2f, GameOff.Station.DoorAccess.Locked),
            new Door("Door_Hab206", true, -37.6f, 23f, 1.2f, GameOff.Station.DoorAccess.Locked),
            new Door("Door_Hab207", true, -35.4f, 27f, 1.2f, GameOff.Station.DoorAccess.Locked),
            new Door("Door_Hab208", true, -37.6f, 27f, 1.2f, GameOff.Station.DoorAccess.Locked),
            new Door("Door_Hab209", true, -35.4f, 31f, 1.2f, GameOff.Station.DoorAccess.Public),
            new Door("Door_Hab210", true, -37.6f, 31f, 1.2f, GameOff.Station.DoorAccess.Public),
            new Door("Door_Hab211", true, -35.4f, 35f, 1.2f, GameOff.Station.DoorAccess.Public),
            new Door("Door_Hab212", true, -37.6f, 35f, 1.2f, GameOff.Station.DoorAccess.Locked),
        };

        public static readonly RoomArea[] Rooms =
        {
            new RoomArea(201, -35.4f, -31.3f, 14f, 18f),
            new RoomArea(202, -41.7f, -37.6f, 14f, 18f),
            new RoomArea(203, -35.4f, -31.3f, 18f, 22f),
            new RoomArea(204, -41.7f, -37.6f, 18f, 22f),
            new RoomArea(205, -35.4f, -31.3f, 22f, 26f),
            new RoomArea(206, -41.7f, -37.6f, 22f, 26f),
            new RoomArea(207, -35.4f, -31.3f, 26f, 30f),
            new RoomArea(208, -41.7f, -37.6f, 26f, 30f),
            new RoomArea(209, -35.4f, -31.3f, 30f, 34f),
            new RoomArea(210, -41.7f, -37.6f, 30f, 34f),
            new RoomArea(211, -35.4f, -31.3f, 34f, 38f),
            new RoomArea(212, -41.7f, -37.6f, 34f, 38f),
        };
    }
}
