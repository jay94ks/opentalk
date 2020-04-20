using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTalk.Tasks;

namespace OpenTalk.UI.Forms
{
    /// <summary>
    /// 색상을 자유롭게 바꿀 수 있는 프로그레스 바입니다.
    /// </summary>
    public partial class ColorizedProgressBar : UserControl
    {
        private Timer m_MarqueeTimer = new Timer();
        private float m_Progress = 0.0f;
        private Brush m_CachedBrush = null;

        public ColorizedProgressBar()
        {
            InitializeComponent();

            m_Progress = 0.0f;
            (m_MarqueeTimer = new Timer() { Interval = 100, Enabled = false })
                .Tick += OnMarqueeUpdate;

            DoubleBuffered = true;
        }
        
        /// <summary>
        /// 프로그레스바의 표시 방식입니다.
        /// (기본값: Marquee)
        /// </summary>
        public ProgressBarStyle ProgressStyle { get; set; } = ProgressBarStyle.Marquee;

        /// <summary>
        /// 수평 프로그레스바가 아닌 수직 프로그레스바가 필요하다면 true를 셋팅 하십시오.
        /// </summary>
        public bool HorizontalProgress { get; set; } = false;

        /// <summary>
        /// 현재 프로그레스 바의 오프셋입니다.
        /// </summary>
        public float Progress {
            get => m_Progress;
            set {
                if (m_Progress != value)
                {
                    m_Progress = value;

                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            if (Visible)
                                Invalidate();
                        }));
                    }
                    else if (Visible)
                        Invalidate();
                }
            }
        }

        /// <summary>
        /// 프로그레스바의 최솟 값입니다.
        /// </summary>
        public float ProgressMinimum { get; set; } = 0;

        /// <summary>
        /// 프로그레스바의 최대 값입니다.
        /// </summary>
        public float ProgressMaximum { get; set; } =  100;

        /// <summary>
        /// Marquee 모드 일 때 진행 속도입니다.
        /// (기본값: 100, --> 0.1s)
        /// </summary>
        public int MarqueeSpeed {
            get => m_MarqueeTimer.Interval;
            set => m_MarqueeTimer.Interval = value;
        }

        /// <summary>
        /// 이 프로그레스바가 적재되면 실행됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            if (ProgressStyle == ProgressBarStyle.Marquee && !DesignMode)
                m_MarqueeTimer.Start();

            base.OnLoad(e);
        }

        /// <summary>
        /// 브러쉬 인스턴스가 없으면 새로 만듭니다.
        /// </summary>
        private void EnsureBrushInstance()
        {
            if (m_CachedBrush == null)
                m_CachedBrush = new SolidBrush(ForeColor);
        }

        /// <summary>
        /// 전경 색이 바뀌면 브러쉬도 새로 만듭니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);

            if (m_CachedBrush != null)
                m_CachedBrush.Dispose();

            m_CachedBrush = new SolidBrush(ForeColor);
        }

        /// <summary>
        /// 이 컨트롤을 그려야 할 때 호출됩니다.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            float NormalizedOffset = 0.0f;
            
            ClearifySettings();

            if (ProgressMaximum - ProgressMinimum <= 0.0f)
            {
                // ERROR: 0으로 나눌 수 없습니다 에러.
                return;
            }

            EnsureBrushInstance();
            e.Graphics.Clear(BackColor);

            NormalizedOffset = ((Progress <= ProgressMinimum ? ProgressMinimum :
                (Progress >= ProgressMaximum ? ProgressMaximum : Progress)
                - ProgressMinimum) / (ProgressMaximum - ProgressMinimum));

            // Marquee가 아니면 그대로.
            if (ProgressStyle != ProgressBarStyle.Marquee)
            {
                NormalizedOffset *= (HorizontalProgress ? Width : Height);

                if (HorizontalProgress)
                    e.Graphics.FillRectangle(m_CachedBrush, 0, 0, Width, NormalizedOffset);

                else
                    e.Graphics.FillRectangle(m_CachedBrush, 0, 0, NormalizedOffset, Height);
            }

            else
            {
                float BarSize = (HorizontalProgress ? Height * 0.25f : Width * 0.25f);
                float LeftSize = 0.0f;

                if (HorizontalProgress)
                {
                    LeftSize = NormalizedOffset * Height;
                    e.Graphics.FillRectangle(m_CachedBrush, 0, LeftSize, Width, BarSize);

                    LeftSize = Math.Max(LeftSize + BarSize - Height, 0);
                    if (LeftSize > 0.0f)
                        e.Graphics.FillRectangle(m_CachedBrush, 0, 0, Width, LeftSize);
                }

                else
                {
                    LeftSize = NormalizedOffset * Width;
                    e.Graphics.FillRectangle(m_CachedBrush, LeftSize, 0, BarSize, Height);

                    LeftSize = Math.Max(LeftSize + BarSize - Width, 0);
                    if (LeftSize > 0.0f)
                        e.Graphics.FillRectangle(m_CachedBrush, 0, 0, LeftSize, Height);
                }
            }

            //HorizontalProgress
            base.OnPaint(e);
        }

        private void ClearifySettings()
        {
            if (ProgressMinimum > ProgressMaximum)
            {
                float Temp = ProgressMaximum;
                ProgressMaximum = ProgressMinimum;
                ProgressMinimum = Temp;
            }
        }

        /// <summary>
        /// Marquee로 렌더링 될 때 상태를 업데이트 합니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMarqueeUpdate(object sender, EventArgs e)
        {
            m_Progress += 1.0f;

            if (m_Progress > ProgressMaximum)
                m_Progress = ProgressMinimum;

            try
            {
                if (Visible)
                    Invalidate();
            }
            catch { }
        }
    }
}
