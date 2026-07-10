namespace kudwa_focus;

public sealed class main_form : Form
{
    private static readonly Color deep_colour = Color.FromArgb(14, 17, 16);
    private static readonly Color panel_colour = Color.FromArgb(22, 26, 24);
    private static readonly Color surface_colour = Color.FromArgb(34, 39, 37);
    private static readonly Color ivory_colour = Color.FromArgb(246, 241, 229);
    private static readonly Color muted_colour = Color.FromArgb(173, 181, 176);
    private static readonly Color orange_colour = Color.FromArgb(255, 91, 31);
    private static readonly Color amber_colour = Color.FromArgb(255, 184, 0);
    private static readonly Color red_colour = Color.FromArgb(255, 45, 45);

    private readonly countdown_engine engine = new();
    private readonly audio_controller audio = new();
    private readonly System.Windows.Forms.Timer interface_timer;
    private readonly stage_panel stage;
    private readonly Label activity_label;
    private readonly Label timer_label;
    private readonly Label status_label;
    private readonly Label end_time_label;
    private readonly Button start_button;
    private readonly Button reset_button;
    private readonly Button sound_button;
    private readonly Button full_screen_button;
    private readonly TrackBar volume_slider;

    private bool is_muted;
    private bool is_full_screen;
    private bool has_finished;
    private Rectangle previous_bounds;
    private FormBorderStyle previous_border_style;
    private FormWindowState previous_window_state;
    private double animation_phase;

    public main_form()
    {
        Text = "KUDWA™ Focus";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1_000, 680);
        Size = new Size(1_360, 820);
        BackColor = deep_colour;
        ForeColor = ivory_colour;
        Font = new Font("Segoe UI", 10F);
        KeyPreview = true;

        stage = new stage_panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
        activity_label = create_activity_label();
        timer_label = create_timer_label();
        status_label = create_status_label();
        end_time_label = create_end_time_label();
        start_button = create_action_button("START", true, 150);
        reset_button = create_action_button("RESET", false, 118);
        sound_button = create_top_button(audio.is_available ? "SOUND ON" : "NO AUDIO");
        full_screen_button = create_top_button("FULL SCREEN");
        volume_slider = create_volume_slider();

        build_interface();

        interface_timer = new System.Windows.Forms.Timer { Interval = 50 };
        interface_timer.Tick += update_timer;
        interface_timer.Start();

        engine.select(timer_catalog.presets[0].name, timer_catalog.presets[0].duration);
        update_screen();

        start_button.Click += (_, _) => toggle_start_pause();
        reset_button.Click += (_, _) => reset_timer();
        sound_button.Click += (_, _) => toggle_sound();
        full_screen_button.Click += (_, _) => toggle_full_screen();
        volume_slider.ValueChanged += (_, _) => apply_volume();
        KeyDown += handle_key_down;
        Resize += (_, _) => resize_type();
        FormClosed += (_, _) => audio.Dispose();
    }

    private void build_interface()
    {
        var top_bar = build_top_bar();
        var side_bar = build_side_bar();
        var stage_content = build_stage_content();
        stage.Controls.Add(stage_content);

        var body = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            RowCount = 1
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 292F));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        body.Controls.Add(side_bar, 0, 0);
        body.Controls.Add(stage, 1, 0);

        var root = new TableLayoutPanel
        {
            BackColor = deep_colour,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            RowCount = 2
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.Controls.Add(top_bar, 0, 0);
        root.Controls.Add(body, 0, 1);
        Controls.Add(root);
    }

    private Control build_top_bar()
    {
        var wordmark = new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "KUDWA™  /  FOCUS",
            Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold),
            ForeColor = ivory_colour,
            Margin = new Padding(0)
        };

        var principle = new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "KNOWLEDGE  •  UNDERSTANDING  •  DISCERNMENT",
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            ForeColor = muted_colour,
            Margin = new Padding(18, 5, 0, 0)
        };

        var brand = new FlowLayoutPanel
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };
        brand.Controls.Add(wordmark);
        brand.Controls.Add(principle);

        var volume_label = new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = "VOLUME",
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            ForeColor = muted_colour,
            Margin = new Padding(0, 14, 2, 0)
        };

        sound_button.Enabled = audio.is_available;

        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };
        actions.Controls.Add(volume_label);
        actions.Controls.Add(volume_slider);
        actions.Controls.Add(sound_button);
        actions.Controls.Add(full_screen_button);

        var top_bar = new TableLayoutPanel
        {
            BackColor = Color.FromArgb(8, 10, 9),
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(22, 0, 18, 0),
            RowCount = 1,
            Margin = new Padding(0)
        };
        top_bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        top_bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        top_bar.Controls.Add(brand, 0, 0);
        top_bar.Controls.Add(actions, 1, 0);
        return top_bar;
    }

    private Control build_side_bar()
    {
        var title = new Label
        {
            AutoSize = false,
            Size = new Size(244, 34),
            Text = "SESSION TIMERS",
            Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
            ForeColor = orange_colour,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 0, 4)
        };

        var description = new Label
        {
            AutoSize = false,
            Size = new Size(244, 46),
            Text = "Choose a moment, then put it on screen.",
            ForeColor = muted_colour,
            Margin = new Padding(0, 0, 0, 12)
        };

        var flow = new FlowLayoutPanel
        {
            AutoScroll = true,
            BackColor = panel_colour,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(22, 24, 22, 18),
            WrapContents = false,
            Margin = new Padding(0)
        };
        flow.Controls.Add(title);
        flow.Controls.Add(description);

        foreach (var preset in timer_catalog.presets)
        {
            flow.Controls.Add(create_preset_button(preset));
        }

        var custom_button = create_preset_style_button("CUSTOM ACTIVITY\nNAME + DURATION", true);
        custom_button.Click += (_, _) => open_custom_timer();
        flow.Controls.Add(custom_button);

        var shortcut_hint = new Label
        {
            AutoSize = false,
            Size = new Size(244, 70),
            Text = "SPACE  start / pause\nF11  full screen     M  mute\n+ / −  adjust one minute",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(125, 134, 129),
            Margin = new Padding(0, 10, 0, 0)
        };
        flow.Controls.Add(shortcut_hint);
        return flow;
    }

    private Control build_stage_content()
    {
        var controls = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };

        var minus_button = create_action_button("− 1 MIN", false, 118);
        var plus_button = create_action_button("+ 1 MIN", false, 118);
        minus_button.Click += (_, _) => adjust_timer(TimeSpan.FromMinutes(-1));
        plus_button.Click += (_, _) => adjust_timer(TimeSpan.FromMinutes(1));
        controls.Controls.Add(minus_button);
        controls.Controls.Add(start_button);
        controls.Controls.Add(reset_button);
        controls.Controls.Add(plus_button);

        var controls_holder = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 3,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 26, 0, 0)
        };
        controls_holder.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        controls_holder.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        controls_holder.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        controls_holder.Controls.Add(controls, 1, 0);

        var layout = new TableLayoutPanel
        {
            BackColor = Color.Transparent,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(42, 20, 42, 34),
            RowCount = 7,
            Margin = new Padding(0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
        layout.Controls.Add(activity_label, 0, 1);
        layout.Controls.Add(timer_label, 0, 2);
        layout.Controls.Add(status_label, 0, 3);
        layout.Controls.Add(end_time_label, 0, 4);
        layout.Controls.Add(controls_holder, 0, 5);
        return layout;
    }

    private Button create_preset_button(timer_preset preset)
    {
        var duration_text = preset.duration.TotalMinutes >= 60
            ? $"{preset.duration.TotalHours:0} HOUR"
            : $"{preset.duration.TotalMinutes:0} MIN";
        var button = create_preset_style_button($"{preset.name.ToUpperInvariant()}\n{duration_text}", false);
        button.Click += (_, _) => select_timer(preset.name, preset.duration);
        return button;
    }

    private static Button create_preset_style_button(string text, bool is_custom)
    {
        var button = new Button
        {
            BackColor = is_custom ? Color.FromArgb(50, 31, 21) : surface_colour,
            Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.Flat,
            ForeColor = is_custom ? orange_colour : ivory_colour,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            Height = 64,
            Margin = new Padding(0, 0, 0, 9),
            Padding = new Padding(13, 0, 0, 0),
            Size = new Size(244, 64),
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = is_custom ? orange_colour : Color.FromArgb(58, 65, 61);
        button.FlatAppearance.BorderSize = 1;
        return button;
    }

    private static Label create_activity_label()
    {
        return new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 29F, FontStyle.Bold),
            ForeColor = ivory_colour,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0)
        };
    }

    private static Label create_timer_label()
    {
        return new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Black", 126F, FontStyle.Bold),
            ForeColor = ivory_colour,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0)
        };
    }

    private static Label create_status_label()
    {
        return new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
            ForeColor = orange_colour,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 0, 0, 6)
        };
    }

    private static Label create_end_time_label()
    {
        return new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            ForeColor = muted_colour,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0)
        };
    }

    private static Button create_action_button(string text, bool is_primary, int width)
    {
        var button = new Button
        {
            BackColor = is_primary ? orange_colour : surface_colour,
            Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            Height = 50,
            Margin = new Padding(5, 0, 5, 0),
            Size = new Size(width, 50),
            Text = text,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = is_primary ? orange_colour : Color.FromArgb(72, 80, 76);
        return button;
    }

    private static Button create_top_button(string text)
    {
        var button = new Button
        {
            AutoSize = true,
            BackColor = surface_colour,
            Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.Flat,
            ForeColor = ivory_colour,
            Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
            Height = 36,
            Margin = new Padding(8, 6, 0, 0),
            Padding = new Padding(10, 0, 10, 0),
            Text = text,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(72, 80, 76);
        return button;
    }

    private static TrackBar create_volume_slider()
    {
        return new TrackBar
        {
            AutoSize = false,
            BackColor = Color.FromArgb(8, 10, 9),
            LargeChange = 10,
            Maximum = 100,
            Minimum = 0,
            SmallChange = 5,
            TickStyle = TickStyle.None,
            Value = 72,
            Size = new Size(110, 34),
            Margin = new Padding(4, 8, 0, 0)
        };
    }

    private void select_timer(string name, TimeSpan duration)
    {
        if (!confirm_switch_if_running())
        {
            return;
        }

        audio.stop();
        engine.select(name, duration);
        has_finished = false;
        update_screen();
    }

    private void open_custom_timer()
    {
        if (!confirm_switch_if_running())
        {
            return;
        }

        using var dialog = new custom_timer_dialog();

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            audio.stop();
            engine.select(dialog.activity_name, dialog.duration);
            has_finished = false;
            update_screen();
        }
    }

    private bool confirm_switch_if_running()
    {
        if (!engine.is_running)
        {
            return true;
        }

        return MessageBox.Show(
            this,
            "The current countdown is running. Switch timers?",
            "KUDWA Focus",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }

    private void toggle_start_pause()
    {
        if (engine.is_running)
        {
            engine.pause();
            audio.pause();
            update_screen();
            return;
        }

        if (has_finished || engine.is_complete)
        {
            engine.reset();
            audio.stop();
            has_finished = false;
        }

        engine.start();
        audio.play_for(engine.remaining, effective_volume());
        update_screen();
    }

    private void reset_timer()
    {
        engine.reset();
        audio.stop();
        has_finished = false;
        update_screen();
    }

    private void adjust_timer(TimeSpan change)
    {
        engine.adjust(change);
        has_finished = false;

        if (engine.is_running)
        {
            audio.play_for(engine.remaining, effective_volume(), true);
        }

        update_screen();
    }

    private void update_timer(object? sender, EventArgs event_args)
    {
        animation_phase += interface_timer.Interval / 1_000.0;

        if (engine.is_running && engine.remaining <= TimeSpan.Zero)
        {
            engine.complete();
            has_finished = true;
        }
        else if (engine.is_running)
        {
            var required_phase = timer_math.phase_for(engine.remaining, false);

            if (required_phase != audio.current_phase)
            {
                audio.play_for(engine.remaining, effective_volume(), true);
            }
        }

        update_screen();
    }

    private void update_screen()
    {
        var remaining = engine.remaining;
        var display = current_display_phase(remaining);
        activity_label.Text = engine.activity_name.ToUpperInvariant();
        timer_label.Text = has_finished ? "TIME!" : timer_math.format_remaining(remaining);
        start_button.Text = engine.is_running ? "PAUSE" : has_finished ? "RESTART" : "START";
        reset_button.Enabled = remaining != engine.total_duration || has_finished;

        (status_label.Text, status_label.ForeColor, timer_label.ForeColor) = display switch
        {
            display_phase.complete => ("LET'S BRING THE ROOM BACK TOGETHER", red_colour, red_colour),
            display_phase.final_ten => ("THE FINAL TEN — HERE WE GO!", red_colour, ivory_colour),
            display_phase.final_minute => ("FINAL MINUTE — ENERGY UP", amber_colour, ivory_colour),
            display_phase.paused => ("PAUSED", muted_colour, ivory_colour),
            display_phase.running => ("MAKE THE MOMENT COUNT", orange_colour, ivory_colour),
            _ => ("READY WHEN YOU ARE", orange_colour, ivory_colour)
        };

        end_time_label.Text = has_finished
            ? "TIME IS EVIDENCE — MOVE WITH PURPOSE"
            : $"END TIME  •  {DateTime.Now.Add(remaining):HH:mm}";

        stage.update_visual(
            timer_math.progress(engine.total_duration, remaining),
            display,
            animation_phase * animation_speed(display));
    }

    private display_phase current_display_phase(TimeSpan remaining)
    {
        if (has_finished)
        {
            return display_phase.complete;
        }

        if (engine.is_running && remaining <= TimeSpan.FromSeconds(10))
        {
            return display_phase.final_ten;
        }

        if (engine.is_running && remaining <= TimeSpan.FromSeconds(60))
        {
            return display_phase.final_minute;
        }

        if (engine.is_running)
        {
            return display_phase.running;
        }

        return remaining < engine.total_duration ? display_phase.paused : display_phase.ready;
    }

    private static double animation_speed(display_phase phase)
    {
        return phase switch
        {
            display_phase.final_ten or display_phase.complete => 2.6,
            display_phase.final_minute => 1.1,
            _ => 0.22
        };
    }

    private void toggle_sound()
    {
        if (!audio.is_available)
        {
            return;
        }

        is_muted = !is_muted;
        sound_button.Text = is_muted ? "MUTED" : "SOUND ON";
        sound_button.ForeColor = is_muted ? amber_colour : ivory_colour;
        apply_volume();
    }

    private void apply_volume()
    {
        audio.set_volume(effective_volume());
    }

    private int effective_volume()
    {
        return is_muted ? 0 : volume_slider.Value * 10;
    }

    private void toggle_full_screen()
    {
        if (!is_full_screen)
        {
            previous_bounds = Bounds;
            previous_border_style = FormBorderStyle;
            previous_window_state = WindowState;
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.FromControl(this).Bounds;
            full_screen_button.Text = "EXIT FULL SCREEN";
            is_full_screen = true;
        }
        else
        {
            FormBorderStyle = previous_border_style;
            WindowState = previous_window_state;
            Bounds = previous_bounds;
            full_screen_button.Text = "FULL SCREEN";
            is_full_screen = false;
        }

        resize_type();
    }

    private void resize_type()
    {
        if (stage.ClientSize.Width <= 0 || stage.ClientSize.Height <= 0)
        {
            return;
        }

        var timer_size = Math.Clamp(
            Math.Min(stage.ClientSize.Width * 0.145F, stage.ClientSize.Height * 0.26F),
            72F,
            180F);
        timer_label.Font = new Font("Segoe UI Black", timer_size, FontStyle.Bold);
        activity_label.Font = new Font("Segoe UI Semibold", Math.Clamp(timer_size * 0.23F, 22F, 42F), FontStyle.Bold);
    }

    private void handle_key_down(object? sender, KeyEventArgs event_args)
    {
        switch (event_args.KeyCode)
        {
            case Keys.Space:
                toggle_start_pause();
                event_args.SuppressKeyPress = true;
                break;
            case Keys.F11:
                toggle_full_screen();
                event_args.SuppressKeyPress = true;
                break;
            case Keys.Escape when is_full_screen:
                toggle_full_screen();
                event_args.SuppressKeyPress = true;
                break;
            case Keys.M:
                toggle_sound();
                event_args.SuppressKeyPress = true;
                break;
            case Keys.R:
                reset_timer();
                event_args.SuppressKeyPress = true;
                break;
            case Keys.Add:
            case Keys.Oemplus:
                adjust_timer(TimeSpan.FromMinutes(1));
                event_args.SuppressKeyPress = true;
                break;
            case Keys.Subtract:
            case Keys.OemMinus:
                adjust_timer(TimeSpan.FromMinutes(-1));
                event_args.SuppressKeyPress = true;
                break;
        }
    }
}
