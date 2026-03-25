using System.Collections;
using UnityEngine;

/// <summary>
/// Manages all world-space visuals for a battle: background, trainer sprite,
/// monster sprites, and every animation sequence.
///
/// Sprites are placeholder coloured shapes generated at runtime — swap out
/// frontSprite / backSprite on PokemonData and trainerSprite on TrainerData
/// to use real art without touching this class.
///
/// All public coroutines yield until the animation is complete so BattleManager
/// can simply "yield return" them inline.
/// </summary>
public class BattleVisuals : MonoBehaviour
{
    private static BattleVisuals _instance;
    public static BattleVisuals GetInstanceST() => _instance;

    // ── Runtime sprite objects ────────────────────────────────────────
    private GameObject   _backgroundGO;
    private GameObject   _groundLineGO;
    private GameObject   _trainerGO;
    private SpriteRenderer _trainerSR;
    private GameObject   _enemyMonGO;
    private SpriteRenderer _enemyMonSR;
    private GameObject   _playerMonGO;
    private SpriteRenderer _playerMonSR;
    private GameObject   _screenFlashGO;
    private SpriteRenderer _screenFlashSR;

    // ── Cached battle data ────────────────────────────────────────────
    private bool         _isWild;
    private BattleUI     _battleUI;

    // ── World-space positions (calibrated for ortho size 4, portrait) ─
    // Off-screen
    private static readonly Vector3 PosOffRight = new Vector3( 6f, 0f, 0f);
    private static readonly Vector3 PosOffLeft  = new Vector3(-6f, 0f, 0f);
    // Trainer stands on the right half
    private static readonly Vector3 PosTrainer  = new Vector3( 1.3f, -0.6f, 0f);
    // Monsters
    private static readonly Vector3 PosEnemyMon = new Vector3( 1.5f,  0.9f, 0f);
    private static readonly Vector3 PosPlayerMon= new Vector3(-1.5f, -0.6f, 0f);

    // ── Monster scale (world units) ───────────────────────────────────
    // Enemy shows its "front" (smaller), player shows its "back" (larger).
    private static readonly Vector3 ScaleEnemyMon  = new Vector3(1.6f, 1.6f, 1f);
    private static readonly Vector3 ScalePlayerMon = new Vector3(2.0f, 2.0f, 1f);
    private static readonly Vector3 ScaleTrainer   = new Vector3(1.1f, 2.2f, 1f);

    private void Awake() => _instance = this;

    // ─────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────

    /// <summary>Creates all sprite objects. Call before any animation coroutine.</summary>
    public void InitST(PokemonInstance playerMon, PokemonInstance enemyMon,
                       bool isWild, TrainerData trainer = null)
    {
        _isWild   = isWild;
        _battleUI = FindAnyObjectByType<BattleUI>();

        BuildSceneST(playerMon, enemyMon, trainer);
    }

    /// <summary>Full intro sequence: trainer reveal (trainer battles) then monsters pop in.</summary>
    public IEnumerator PlayIntroSequenceST()
    {
        // Both monsters start hidden
        _enemyMonGO.SetActive(false);
        _playerMonGO.SetActive(false);

        if (!_isWild && _trainerGO != null)
        {
            // Trainer slides in from off-right
            _trainerGO.SetActive(true);
            _trainerGO.transform.position = PosOffRight;
            yield return SlideST(_trainerGO.transform, PosTrainer, 0.45f, Ease.Out);

            _battleUI?.AppendLogST("The enemy trainer appeared!");
            yield return new WaitForSeconds(1.4f);

            // Trainer tosses first monster, slides out left
            _battleUI?.AppendLogST("Go!");
            yield return SlideST(_trainerGO.transform, PosOffLeft, 0.4f, Ease.In);
            _trainerGO.SetActive(false);
        }

        // Enemy monster pops in
        _enemyMonGO.SetActive(true);
        yield return PopInST(_enemyMonGO.transform);

        string enemyName = _enemyMonGO.name.Replace("_EnemyMon", "");
        _battleUI?.AppendLogST(_isWild
            ? $"A wild {enemyName} appeared!"
            : $"The enemy sent out {enemyName}!");
        yield return new WaitForSeconds(0.6f);

        // Player monster pops in
        _playerMonGO.SetActive(true);
        yield return PopInST(_playerMonGO.transform);

        string playerName = _playerMonGO.name.Replace("_PlayerMon", "");
        _battleUI?.AppendLogST($"Go, {playerName}!");
        yield return new WaitForSeconds(0.4f);
    }

    /// <summary>
    /// Plays a skill animation over the target.
    /// Physical: target recoil. Special: screen flash + target blink. Status: target pulse.
    /// Extend MoveCategory handling here when custom per-move VFX are added.
    /// </summary>
    public IEnumerator PlaySkillAnimationST(bool hitEnemy, MoveData move)
    {
        SpriteRenderer target = hitEnemy ? _enemyMonSR : _playerMonSR;
        if (target == null) yield break;

        switch (move.category)
        {
            case MoveCategory.Physical:
                yield return RecoilST(target.transform, hitEnemy ? Vector3.left : Vector3.right);
                break;

            case MoveCategory.Special:
                yield return ScreenFlashST(TypeFlashColour(move.type), 0.15f);
                yield return BlinkST(target, 4, 0.07f);
                break;

            case MoveCategory.Status:
                yield return PulseST(target, StatusColour(move.statusEffect), 0.6f);
                break;
        }
    }

    /// <summary>Faint animation: target drops and fades out.</summary>
    public IEnumerator PlayFaintAnimationST(bool isEnemy)
    {
        GameObject go = isEnemy ? _enemyMonGO : _playerMonGO;
        SpriteRenderer sr = isEnemy ? _enemyMonSR : _playerMonSR;
        if (go == null) yield break;

        Vector3 start = go.transform.position;
        Vector3 end   = start + Vector3.down * 2f;
        float dur = 0.55f, t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            go.transform.position = Vector3.Lerp(start, end, EaseIn(p));
            Color c = sr.color; c.a = 1f - p; sr.color = c;
            yield return null;
        }
        go.SetActive(false);
    }

    /// <summary>Victory animation: winner bounces twice.</summary>
    public IEnumerator PlayVictoryAnimationST(bool isEnemy)
    {
        GameObject go = isEnemy ? _enemyMonGO : _playerMonGO;
        if (go == null || !go.activeSelf) yield break;
        yield return HopST(go.transform, 2, 0.35f, 0.18f);
    }

    /// <summary>
    /// End-of-battle outro: for trainer battles the trainer re-enters,
    /// delivers an outcome line, then leaves.
    /// </summary>
    public IEnumerator PlayOutroSequenceST(BattleResult result)
    {
        if (_isWild) yield break;
        if (_trainerGO == null) yield break;

        _trainerGO.SetActive(true);
        _trainerGO.transform.position = PosOffLeft;
        yield return SlideST(_trainerGO.transform, PosTrainer, 0.5f, Ease.Out);

        string line = result switch
        {
            BattleResult.Win  => "Hmph... you got lucky.",
            BattleResult.Lose => "Ha! You never stood a chance!",
            _                 => "..."
        };
        _battleUI?.AppendLogST(line);
        yield return new WaitForSeconds(2f);

        yield return SlideST(_trainerGO.transform, PosOffRight, 0.45f, Ease.In);
        _trainerGO.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    // Scene construction
    // ─────────────────────────────────────────────────────────────────

    private void BuildSceneST(PokemonInstance playerMon, PokemonInstance enemyMon,
                               TrainerData trainer)
    {
        // Background (sky top / ground bottom)
        _backgroundGO = MakeSpriteGO("Background",
            MakeBackgroundSprite(new Color(0.08f, 0.07f, 0.12f),
                                 new Color(0.18f, 0.15f, 0.12f)),
            new Vector3(0, 0, 8f), new Vector3(10f, 20f, 1f), -5);

        // Thin ground divider
        _groundLineGO = MakeSpriteGO("GroundLine",
            MakeSolidRect(1, 1, new Color(0.4f, 0.35f, 0.3f)),
            new Vector3(0, -0.3f, 4f), new Vector3(6f, 0.04f, 1f), -4);

        // Enemy trainer
        Color trainerCol = new Color(0.25f, 0.22f, 0.28f);
        Sprite trainerSprite = trainer?.trainerSprite != null
            ? trainer.trainerSprite
            : MakeTrainerShape(trainerCol);

        _trainerGO = MakeSpriteGO("EnemyTrainer", trainerSprite,
            PosOffRight, ScaleTrainer, 0);
        _trainerSR = _trainerGO.GetComponent<SpriteRenderer>();
        _trainerGO.SetActive(false);

        // Enemy monster
        Color enemyCol = enemyMon?.data != null
            ? TypeColour(enemyMon.data.primaryType)
            : Color.grey;
        Sprite enemySprite = enemyMon?.data?.frontSprite != null
            ? enemyMon.data.frontSprite
            : MakeMonsterShape(enemyCol);

        string enemyName = enemyMon?.nickname ?? "???";
        _enemyMonGO = MakeSpriteGO(enemyName + "_EnemyMon", enemySprite,
            PosEnemyMon, ScaleEnemyMon, 1);
        _enemyMonSR = _enemyMonGO.GetComponent<SpriteRenderer>();

        // Player monster (back view = larger, flipped)
        Color playerCol = playerMon?.data != null
            ? TypeColour(playerMon.data.primaryType)
            : new Color(0.4f, 0.7f, 0.4f);
        Sprite playerSprite = playerMon?.data?.backSprite != null
            ? playerMon.data.backSprite
            : MakeMonsterShape(playerCol, back: true);

        string playerName = playerMon?.nickname ?? "???";
        _playerMonGO = MakeSpriteGO(playerName + "_PlayerMon", playerSprite,
            PosPlayerMon, ScalePlayerMon, 1);
        _playerMonSR = _playerMonGO.GetComponent<SpriteRenderer>();

        // Full-screen flash overlay (starts transparent, sorting on top)
        _screenFlashGO = MakeSpriteGO("ScreenFlash",
            MakeSolidRect(1, 1, Color.white),
            new Vector3(0, 0, -1f), new Vector3(10f, 20f, 1f), 20);
        _screenFlashSR = _screenFlashGO.GetComponent<SpriteRenderer>();
        Color clear = Color.white; clear.a = 0f;
        _screenFlashSR.color = clear;
    }

    // ─────────────────────────────────────────────────────────────────
    // Animation primitives
    // ─────────────────────────────────────────────────────────────────

    private enum Ease { Linear, In, Out }

    private IEnumerator SlideST(Transform t, Vector3 dest, float dur, Ease ease = Ease.Linear)
    {
        Vector3 start = t.position;
        for (float e = 0f; e < dur; e += Time.deltaTime)
        {
            float p = ApplyEase(e / dur, ease);
            t.position = Vector3.Lerp(start, dest, p);
            yield return null;
        }
        t.position = dest;
    }

    private IEnumerator PopInST(Transform t)
    {
        t.localScale = Vector3.zero;
        float dur = 0.35f;
        for (float e = 0f; e < dur; e += Time.deltaTime)
        {
            float p = e / dur;
            // Spring: overshoot at 70% then settle
            float s = p < 0.7f
                ? Mathf.Lerp(0f, 1.15f, p / 0.7f)
                : Mathf.Lerp(1.15f, 1f, (p - 0.7f) / 0.3f);
            t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    private IEnumerator RecoilST(Transform t, Vector3 dir)
    {
        Vector3 origin = t.position;
        Vector3 peak   = origin + dir * 0.25f;
        float   half   = 0.07f;
        for (float e = 0f; e < half; e += Time.deltaTime)
        { t.position = Vector3.Lerp(origin, peak, e / half); yield return null; }
        for (float e = 0f; e < half; e += Time.deltaTime)
        { t.position = Vector3.Lerp(peak, origin, e / half); yield return null; }
        t.position = origin;
    }

    private IEnumerator BlinkST(SpriteRenderer sr, int count, float interval)
    {
        for (int i = 0; i < count * 2; i++)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(interval);
        }
        sr.enabled = true;
    }

    private IEnumerator PulseST(SpriteRenderer sr, Color pulseCol, float dur)
    {
        Color original = sr.color;
        float half = dur * 0.5f;
        for (float e = 0f; e < half; e += Time.deltaTime)
        { sr.color = Color.Lerp(original, pulseCol, e / half); yield return null; }
        for (float e = 0f; e < half; e += Time.deltaTime)
        { sr.color = Color.Lerp(pulseCol, original, e / half); yield return null; }
        sr.color = original;
    }

    private IEnumerator ScreenFlashST(Color col, float dur)
    {
        _screenFlashSR.color = col;
        for (float e = 0f; e < dur; e += Time.deltaTime)
        {
            Color c = col; c.a = Mathf.Lerp(col.a, 0f, e / dur);
            _screenFlashSR.color = c;
            yield return null;
        }
        Color clear = col; clear.a = 0f;
        _screenFlashSR.color = clear;
    }

    private IEnumerator HopST(Transform t, int hops, float hopDur, float height)
    {
        Vector3 basePos = t.position;
        for (int h = 0; h < hops; h++)
        {
            for (float e = 0f; e < hopDur; e += Time.deltaTime)
            {
                float arc = Mathf.Sin(e / hopDur * Mathf.PI) * height;
                t.position = basePos + Vector3.up * arc;
                yield return null;
            }
            t.position = basePos;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Sprite factories
    // ─────────────────────────────────────────────────────────────────

    private static GameObject MakeSpriteGO(string name, Sprite sprite,
        Vector3 pos, Vector3 scale, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.position   = pos;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = sortOrder;
        return go;
    }

    /// <summary>
    /// Monster placeholder: coloured circle with two white dot eyes.
    /// back=true adds a slightly darker tint (back-view).
    /// </summary>
    private static Sprite MakeMonsterShape(Color color, bool back = false)
    {
        if (back) color *= 0.85f;
        const int S = 64;
        var tex    = new Texture2D(S, S, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float r = S * 0.47f, cx = S * 0.5f, cy = S * 0.5f;
        float eyeR = S * 0.07f;
        float ex1 = cx - S * 0.14f, ey = cy + S * 0.12f;
        float ex2 = cx + S * 0.14f;

        var pixels = new Color[S * S];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % S, y = i / S;
            float dx = x - cx, dy = y - cy;
            if (dx * dx + dy * dy > r * r) { pixels[i] = Color.clear; continue; }

            // Shade slightly
            Color c = color * Mathf.Lerp(0.75f, 1.1f, (float)y / S);
            c.a = 1f;

            // Eyes
            float d1 = (x - ex1) * (x - ex1) + (y - ey) * (y - ey);
            float d2 = (x - ex2) * (x - ex2) + (y - ey) * (y - ey);
            if (d1 < eyeR * eyeR || d2 < eyeR * eyeR)
                c = new Color(0.95f, 0.95f, 0.95f);

            pixels[i] = c;
        }
        tex.SetPixels(pixels); tex.Apply();
        // PPU = S so the sprite is exactly 1 unit at scale 1; we scale it via transform
        return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), S);
    }

    /// <summary>
    /// Trainer placeholder: simple humanoid silhouette (head + torso + legs).
    /// </summary>
    private static Sprite MakeTrainerShape(Color color)
    {
        const int W = 32, H = 64;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color[W * H];
        float cx = W * 0.5f;

        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % W, y = i / H;  // NOTE: intentional - y not used to avoid re-declaration
            y = i / W;
            float fy = (float)y / H;

            bool inside = false;
            float dx = x - cx;

            // Head (top 20%)
            if (fy > 0.80f) inside = Mathf.Abs(dx) < W * 0.22f;
            // Neck (18–20%)
            else if (fy > 0.78f) inside = Mathf.Abs(dx) < W * 0.10f;
            // Torso (40–78%)
            else if (fy > 0.40f) inside = Mathf.Abs(dx) < Mathf.Lerp(W * 0.26f, W * 0.22f, (fy - 0.40f) / 0.38f);
            // Legs (0–40%) — two pillars
            else inside = Mathf.Abs(dx - W * 0.13f) < W * 0.11f || Mathf.Abs(dx + W * 0.13f) < W * 0.11f;

            if (inside) { Color c = color; c.a = 1f; pixels[i] = c; }
            else          pixels[i] = Color.clear;
        }
        tex.SetPixels(pixels); tex.Apply();
        // Pivot at bottom-centre so the trainer stands on the ground point
        return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0f), W);
    }

    private static Sprite MakeSolidRect(int w, int h, Color color)
    {
        var tex = new Texture2D(w, h);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 1f);
    }

    /// <summary>Two-tone background: sky top half, ground bottom half.</summary>
    private static Sprite MakeBackgroundSprite(Color sky, Color ground)
    {
        const int W = 2, H = 4;
        var tex = new Texture2D(W, H);
        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
            tex.SetPixel(x, y, y >= H / 2 ? sky : ground);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 1f);
    }

    // ─────────────────────────────────────────────────────────────────
    // Colour tables
    // ─────────────────────────────────────────────────────────────────

    private static Color TypeColour(PokemonType t) => t switch
    {
        PokemonType.Fire     => new Color(1.00f, 0.40f, 0.10f),
        PokemonType.Water    => new Color(0.15f, 0.55f, 1.00f),
        PokemonType.Grass    => new Color(0.30f, 0.80f, 0.30f),
        PokemonType.Electric => new Color(1.00f, 0.90f, 0.10f),
        PokemonType.Ice      => new Color(0.60f, 0.90f, 1.00f),
        PokemonType.Rock     => new Color(0.65f, 0.55f, 0.35f),
        PokemonType.Ground   => new Color(0.85f, 0.70f, 0.40f),
        PokemonType.Poison   => new Color(0.70f, 0.20f, 0.80f),
        PokemonType.Flying   => new Color(0.55f, 0.70f, 1.00f),
        PokemonType.Ghost    => new Color(0.30f, 0.20f, 0.55f),
        PokemonType.Dragon   => new Color(0.40f, 0.20f, 0.85f),
        PokemonType.Dark     => new Color(0.30f, 0.22f, 0.28f),
        PokemonType.Steel    => new Color(0.70f, 0.72f, 0.82f),
        PokemonType.Fairy    => new Color(1.00f, 0.60f, 0.80f),
        PokemonType.Fighting => new Color(0.80f, 0.30f, 0.10f),
        PokemonType.Psychic  => new Color(1.00f, 0.30f, 0.55f),
        PokemonType.Bug      => new Color(0.60f, 0.80f, 0.20f),
        _                    => new Color(0.70f, 0.70f, 0.70f),
    };

    private static Color TypeFlashColour(PokemonType t) => t switch
    {
        PokemonType.Fire     => new Color(1f, 0.5f, 0f, 0.55f),
        PokemonType.Water    => new Color(0f, 0.5f, 1f, 0.55f),
        PokemonType.Grass    => new Color(0f, 0.8f, 0f, 0.45f),
        PokemonType.Electric => new Color(1f, 1f, 0f, 0.65f),
        PokemonType.Ghost    => new Color(0.5f, 0f, 1f, 0.50f),
        PokemonType.Dark     => new Color(0.1f, 0f, 0.2f, 0.60f),
        PokemonType.Psychic  => new Color(1f, 0f, 0.5f, 0.50f),
        _                    => new Color(1f, 1f, 1f, 0.40f),
    };

    private static Color StatusColour(StatusCondition s) => s switch
    {
        StatusCondition.Burned    => new Color(1f, 0.3f, 0f,  0.7f),
        StatusCondition.Poisoned  => new Color(0.6f, 0f, 0.8f, 0.6f),
        StatusCondition.Paralyzed => new Color(1f, 0.9f, 0f,  0.6f),
        StatusCondition.Frozen    => new Color(0.5f, 0.9f, 1f, 0.6f),
        StatusCondition.Asleep    => new Color(0.5f, 0.5f, 0.7f,0.5f),
        _                         => new Color(0.8f, 0.8f, 0.8f,0.4f),
    };

    // ─────────────────────────────────────────────────────────────────
    // Easing helpers
    // ─────────────────────────────────────────────────────────────────

    private static float ApplyEase(float t, Ease ease) => ease switch
    {
        Ease.In  => EaseIn(t),
        Ease.Out => EaseOut(t),
        _        => t,
    };
    private static float EaseIn(float t)  => t * t;
    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}
