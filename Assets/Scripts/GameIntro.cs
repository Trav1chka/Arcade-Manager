using System.Collections;
using UnityEngine;

// Начало игровой сцены: чёрный экран с надписью "Day 1", затем экран проясняется и начинается день.
// В DayManager должна быть выключена галочка Auto Start Day, день запускает этот скрипт.
public class GameIntro : MonoBehaviour
{
    [SerializeField] private DayManager day;
    [SerializeField] private ScreenFader fader;
    [SerializeField] private float fadeInDuration = 2.5f;   // длительность появления картинки, секунд

    private IEnumerator Start()
    {
        // Start срабатывает раньше первого кадра, поэтому вспышки игры до затемнения не будет
        fader.SetBlack();

        yield return fader.ShowDayTitle(day.CurrentDay);

        // День стартует до проявления экрана, поэтому пауза перед первым гостем идёт уже на видимой сцене
        day.StartDay();

        yield return fader.FadeFromBlack(fadeInDuration);
    }
}
