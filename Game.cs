using Godot;
using System;

public partial class StickmanFighter : CharacterBody2D
{
    // Adjustable Game Parameters in Inspector
    [Export] public int PlayerID = 1; // Set to 1 for Player 1, 2 for Player 2
    [Export] public float Speed = 350.0f;
    [Export] public float JumpVelocity = -550.0f;
    [Export] public int MaxHealth = 100;

    public int CurrentHealth;
    
    // Node References
    private Area2D _attackArea;
    private CollisionShape2D _attackCollision;
    private Sprite2D _sprite;

    // Gravity setting from project defaults
    public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;

        // Fetch child nodes
        _attackArea = GetNode<Area2D>("AttackArea");
        _attackCollision = _attackArea.GetNode<CollisionShape2D>("CollisionShape2D");
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");

        // Disable punch hitbox by default
        _attackCollision.Disabled = true;

        // Listen for hit detection on attack area
        _attackArea.BodyEntered += OnAttackHit;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // 1. Apply Gravity
        if (!IsOnFloor())
        {
            velocity.Y += Gravity * (float)delta;
        }

        // 2. Handle Jump & Movement Inputs per Player
        if (PlayerID == 1)
        {
            // Player 1: WASD Controls
            if (Input.IsKeyPressed(Key.W) && IsOnFloor())
            {
                velocity.Y = JumpVelocity;
            }

            float direction = 0;
            if (Input.IsKeyPressed(Key.D)) direction += 1;
            if (Input.IsKeyPressed(Key.A)) direction -= 1;

            velocity.X = direction * Speed;
            UpdateFacingDirection(direction);

            // Player 1 Attack (Key: F)
            if (Input.IsKeyPressed(Key.F) && _attackCollision.Disabled)
            {
                PerformAttack();
            }
        }
        else if (PlayerID == 2)
        {
            // Player 2: Arrow Controls
            if (Input.IsKeyPressed(Key.Up) && IsOnFloor())
            {
                velocity.Y = JumpVelocity;
            }

            float direction = 0;
            if (Input.IsKeyPressed(Key.Right)) direction += 1;
            if (Input.IsKeyPressed(Key.Left)) direction -= 1;

            velocity.X = direction * Speed;
            UpdateFacingDirection(direction);

            // Player 2 Attack (Key: Space / Keypad Enter)
            if (Input.IsKeyPressed(Key.KpEnter) && _attackCollision.Disabled)
            {
                PerformAttack();
            }
        }

        // Apply physics
        Velocity = velocity;
        MoveAndSlide();
    }

    private void UpdateFacingDirection(float direction)
    {
        if (direction < 0)
        {
            _attackArea.Scale = new Vector2(-1, 1);
            if (_sprite != null) _sprite.FlipH = true;
        }
        else if (direction > 0)
        {
            _attackArea.Scale = new Vector2(1, 1);
            if (_sprite != null) _sprite.FlipH = false;
        }
    }

    private async void PerformAttack()
    {
        GD.Print($"Player {PlayerID} Punches!");

        // Enable attack hitbox
        _attackCollision.Disabled = false;

        // Active punch frame window (0.15 seconds)
        await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);

        // Turn off hitbox
        _attackCollision.Disabled = true;
    }

    private void OnAttackHit(Node2D body)
    {
        // Don't hit yourself!
        if (body != this && body is StickmanFighter enemy)
        {
            enemy.TakeDamage(15);
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        GD.Print($"Player {PlayerID} took {damage} damage! Remaining Health: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            GD.Print($"Player {PlayerID} Knocked Out!");
            QueueFree(); // Removes character from scene
        }
    }
}