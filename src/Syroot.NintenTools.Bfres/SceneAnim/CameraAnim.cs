﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Syroot.Maths;
using Syroot.NintenTools.Bfres.Core;

namespace Syroot.NintenTools.Bfres
{
    /// <summary>
    /// Represents an FCAM section in a <see cref="SceneAnim"/> subfile, storing animations controlling camera settings.
    /// </summary>
    [DebuggerDisplay(nameof(CameraAnim) + " {" + nameof(Name) + "}")]
    public class CameraAnim : INamedResData
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private string _name;

        // ---- EVENTS -------------------------------------------------------------------------------------------------

        public event EventHandler NameChanged;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------
        
        public CameraAnimFlags Flags { get; set; }

        public int FrameCount { get; set; }

        public uint BakedSize { get; private set; }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (_name != value)
                {
                    _name = value;
                    NameChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public IList<AnimCurve> Curves { get; private set; }

        public CameraAnimData BaseData { get; set; }

        public INamedResDataList<UserData> UserData { get; private set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            CameraAnimHead head = new CameraAnimHead(loader);
            Flags = head.Flags;
            FrameCount = head.NumFrame;
            BakedSize = head.SizBaked;
            Name = loader.GetName(head.OfsName);
            Curves = loader.LoadList<AnimCurve>(head.OfsCurveList, head.NumCurve);

            if (head.OfsBaseData != 0)
            {
                loader.Position = head.OfsBaseData;
                BaseData = new CameraAnimData(loader);
            }

            UserData = loader.LoadNamedDictList<UserData>(head.OfsUserDataDict);
        }

        void IResData.Reference(ResFileLoader loader)
        {
        }
    }

    /// <summary>
    /// Represents the header of a <see cref="CameraAnim"/> instance.
    /// </summary>
    internal class CameraAnimHead
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FCAM";

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        internal uint Signature;
        internal CameraAnimFlags Flags;
        internal int NumFrame;
        internal byte NumCurve;
        internal ushort NumUserData;
        internal uint SizBaked;
        internal uint OfsName;
        internal uint OfsCurveList;
        internal uint OfsBaseData;
        internal uint OfsUserDataDict;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        public CameraAnimHead(ResFileLoader loader)
        {
            Signature = loader.ReadSignature(_signature);
            Flags = loader.ReadEnum<CameraAnimFlags>(true);
            loader.Seek(2);
            NumFrame = loader.ReadInt32();
            NumCurve = loader.ReadByte();
            loader.Seek(1);
            NumUserData = loader.ReadUInt16();
            SizBaked = loader.ReadUInt32();
            OfsName = loader.ReadOffset();
            OfsCurveList = loader.ReadOffset();
            OfsBaseData = loader.ReadOffset();
            OfsUserDataDict = loader.ReadOffset();
        }
    }

    [Flags]
    public enum CameraAnimFlags : ushort
    {
        BakedCurve = 1 << 0,
        Looping = 1 << 2,
        EulerZXY = 1 << 8,
        Perspective = 1 << 10
    }
}