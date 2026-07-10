namespace kudwa_focus;

internal sealed class custom_timer_dialog : Form
{
    private readonly TextBox name_text_box;
    private readonly NumericUpDown minutes_input;
    private readonly NumericUpDown seconds_input;

    public string activity_name => name_text_box.Text.Trim();
    public TimeSpan duration => TimeSpan.FromMinutes((double)minutes_input.Value)
        + TimeSpan.FromSeconds((double)seconds_input.Value);

    public custom_timer_dialog()
    {
        Text = "Custom activity";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 390);
        BackColor = Color.FromArgb(18, 21, 20);
        ForeColor = Color.FromArgb(246, 241, 229);
        Font = new Font("Segoe UI", 10F);
        ShowInTaskbar = false;

        var title = new Label
        {
            AutoSize = true,
            Text = "NAME THE MOMENT",
            Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 91, 31),
            Margin = new Padding(0, 0, 0, 6)
        };

        var description = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(450, 0),
            Text = "Create a named countdown for a discussion, activity, reflection or challenge.",
            ForeColor = Color.FromArgb(180, 186, 181),
            Margin = new Padding(0, 0, 0, 18)
        };

        var name_label = create_field_label("ACTIVITY NAME");
        name_text_box = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 13F),
            Text = "Activity",
            Margin = new Padding(0, 4, 0, 18)
        };

        minutes_input = create_number_input(10, 0, 240);
        seconds_input = create_number_input(0, 0, 59);

        var time_fields = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 22)
        };
        time_fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        time_fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        time_fields.Controls.Add(create_number_group("MINUTES", minutes_input), 0, 0);
        time_fields.Controls.Add(create_number_group("SECONDS", seconds_input), 1, 0);

        var cancel_button = create_button("CANCEL", false);
        cancel_button.DialogResult = DialogResult.Cancel;
        var create_button_control = create_button("SET TIMER", true);
        create_button_control.Click += confirm;

        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = new Padding(0)
        };
        actions.Controls.Add(create_button_control);
        actions.Controls.Add(cancel_button);

        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(30),
            RowCount = 7
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(description, 0, 1);
        layout.Controls.Add(name_label, 0, 2);
        layout.Controls.Add(name_text_box, 0, 3);
        layout.Controls.Add(create_field_label("DURATION"), 0, 4);
        layout.Controls.Add(time_fields, 0, 5);
        layout.Controls.Add(actions, 0, 6);
        Controls.Add(layout);

        AcceptButton = create_button_control;
        CancelButton = cancel_button;
    }

    private void confirm(object? sender, EventArgs event_args)
    {
        if (string.IsNullOrWhiteSpace(activity_name))
        {
            MessageBox.Show(this, "Give the activity a name.", "KUDWA Focus", MessageBoxButtons.OK, MessageBoxIcon.Information);
            name_text_box.Focus();
            return;
        }

        if (duration <= TimeSpan.Zero)
        {
            MessageBox.Show(this, "Choose a duration greater than zero.", "KUDWA Focus", MessageBoxButtons.OK, MessageBoxIcon.Information);
            minutes_input.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static Label create_field_label(string text)
    {
        return new Label
        {
            AutoSize = true,
            Text = text,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(180, 186, 181),
            Margin = new Padding(0)
        };
    }

    private static NumericUpDown create_number_input(decimal value, decimal minimum, decimal maximum)
    {
        return new NumericUpDown
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold),
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            TextAlign = HorizontalAlignment.Center,
            Margin = new Padding(0, 5, 10, 0)
        };
    }

    private static Control create_number_group(string label, NumericUpDown input)
    {
        var group = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        group.Controls.Add(create_field_label(label), 0, 0);
        group.Controls.Add(input, 0, 1);
        return group;
    }

    private static Button create_button(string text, bool is_primary)
    {
        var button = new Button
        {
            AutoSize = true,
            BackColor = is_primary ? Color.FromArgb(255, 91, 31) : Color.FromArgb(34, 39, 37),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            Height = 42,
            Margin = new Padding(8, 0, 0, 0),
            Padding = new Padding(16, 0, 16, 0),
            Text = text,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = is_primary
            ? Color.FromArgb(255, 91, 31)
            : Color.FromArgb(75, 82, 78);
        return button;
    }
}
