// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class RoomInspector : MultiplayerComposite
    {
        private const float transition_duration = 100;

        private readonly MarginPadding contentPadding = new MarginPadding { Horizontal = 20, Vertical = 10 };

        private ParticipantCountDisplay participantCount;
        private OsuSpriteText name;
        private BeatmapTypeInfo beatmapTypeInfo;
        private ParticipantInfo participantInfo;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private readonly Bindable<RoomStatus> status = new Bindable<RoomStatus>(new RoomStatusNoneSelected());

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"343138"),
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Distributed),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 200,
                                        Masking = true,
                                        Children = new Drawable[]
                                        {
                                            new MultiplayerBackgroundSprite { RelativeSizeAxes = Axes.Both },
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.5f), Color4.Black.Opacity(0)),
                                            },
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding(20),
                                                Children = new Drawable[]
                                                {
                                                    participantCount = new ParticipantCountDisplay
                                                    {
                                                        Anchor = Anchor.TopRight,
                                                        Origin = Anchor.TopRight,
                                                    },
                                                    name = new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.BottomLeft,
                                                        Origin = Anchor.BottomLeft,
                                                        Font = OsuFont.GetFont(size: 30),
                                                        Current = RoomName
                                                    },
                                                },
                                            },
                                        },
                                    },
                                    new StatusColouredContainer(transition_duration)
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 5,
                                        Child = new Box { RelativeSizeAxes = Axes.Both }
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = OsuColour.FromHex(@"28242d"),
                                            },
                                            new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Direction = FillDirection.Vertical,
                                                LayoutDuration = transition_duration,
                                                Padding = contentPadding,
                                                Spacing = new Vector2(0f, 5f),
                                                Children = new Drawable[]
                                                {
                                                    new StatusColouredContainer(transition_duration)
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Child = new StatusText
                                                        {
                                                            Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14),
                                                        }
                                                    },
                                                    beatmapTypeInfo = new BeatmapTypeInfo(),
                                                },
                                            },
                                        },
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = contentPadding,
                                        Children = new Drawable[]
                                        {
                                            participantInfo = new ParticipantInfo(),
                                        },
                                    },
                                },
                            },
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Horizontal = 10 },
                                Child = new ParticipantsList { RelativeSizeAxes = Axes.Both }
                            }
                        }
                    }
                }
            };

            Status.BindValueChanged(_ => updateStatus(), true);
            RoomID.BindValueChanged(_ => updateStatus(), true);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs(status, new CacheInfo(nameof(Room.Status), typeof(Room)));
            return dependencies;
        }

        private void updateStatus()
        {
            if (RoomID.Value == null)
            {
                status.Value = new RoomStatusNoneSelected();

                participantCount.FadeOut(transition_duration);
                beatmapTypeInfo.FadeOut(transition_duration);
                name.FadeOut(transition_duration);
                participantInfo.FadeOut(transition_duration);
            }
            else
            {
                status.Value = Status.Value;

                participantCount.FadeIn(transition_duration);
                beatmapTypeInfo.FadeIn(transition_duration);
                name.FadeIn(transition_duration);
                participantInfo.FadeIn(transition_duration);
            }
        }

        private class RoomStatusNoneSelected : RoomStatus
        {
            public override string Message => @"No Room Selected";
            public override Color4 GetAppropriateColour(OsuColour colours) => colours.Gray8;
        }

        private class StatusText : OsuSpriteText
        {
            [Resolved(typeof(Room), nameof(Room.Status))]
            private Bindable<RoomStatus> status { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                status.BindValueChanged(s => Text = s.NewValue.Message, true);
            }
        }
    }
}
