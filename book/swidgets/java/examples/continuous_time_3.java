import javax.swing.*;
import java.awt.FlowLayout;
import swidgets.*;
import nz.sodium.*;
import nz.sodium.time.*;
import java.util.*;

public class continuous_time_3 {
    public static void main(String[] args) throws InterruptedException {
        setUpLogic();
        loop();
    }

    private static void setUpLogic() {
        JFrame frame = new JFrame("Continuous Time 3");
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setLayout(new BoxLayout(frame.getContentPane(), BoxLayout.Y_AXIS));

        TimerSystem timerSystem = new SecondsTimerSystem();
        Ball ball = new Ball(timerSystem, 200.0);

        SLabel lblSeconds = new SLabel(timerSystem.time.map(double2Unit("s")));
        SLabel lblSpeed = new SLabel(ball.velocity.map(double2Unit(" m/s")));
        SLabel lblHeight = new SLabel(ball.height.map(double2Unit(" m")));

        frame.add(lblSeconds);
        frame.add(lblSpeed);
        frame.add(lblHeight);
        frame.setSize(400, 160);
        frame.setVisible(true);
    }

    static Lambda1<Double, Double> velocity = (seconds) -> 9.81 * seconds;
    static Lambda1<Double, Double> distance = (seconds) -> 0.5 * 9.81 * seconds * seconds;
    static Lambda1<Double, String> double2Unit(String unit) {
        return (d) -> Double.toString(d) + unit;
    }

    private static class Ball {
        private final Cell<Double> secondsAfterCreation;
        private final Cell<Double> height;
        private final Cell<Double> velocity;

        Ball(TimerSystem timerSystem, final double startHeight) {
            Cell<Double> time = timerSystem.time;
            final double secondsOnCreation = time.sample();
            this.secondsAfterCreation = time.map(seconds -> seconds - secondsOnCreation);
            this.height = secondsAfterCreation.map(continuous_time_3.distance).map(distance -> {
                double height = startHeight - distance;
                if(height < 0.0)
                    return 0.0;
                return height;
            });
            this.velocity = secondsAfterCreation.map(continuous_time_3.velocity).lift(height, (v, h) -> h == 0? h : v);
        }

    }

    private static void loop() throws InterruptedException {
        long systemSampleRate = 1000L;
        while(true) {
            //jede Transaktion aktualisiert TimerSystem.time (Sodium spezifisch)
            Transaction.runVoid(() -> {});
            Thread.sleep(systemSampleRate);
        }
    }

}
