import javax.swing.*;
import java.awt.FlowLayout;
import swidgets.*;
import nz.sodium.*;
import nz.sodium.time.*;
import java.util.*;

public class continuous_time_2 {
    public static void main(String[] args) throws InterruptedException {
        setUpLogic();
        loop();
    }

    private static void setUpLogic() {
        JFrame frame = new JFrame("Continuous Time 2");
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setLayout(new BoxLayout(frame.getContentPane(), BoxLayout.Y_AXIS));

        TimerSystem timerSystem = new SecondsTimerSystem();
        Cell<Double> time = timerSystem.time;

        final double startHeightMeter = 4000.0;

        //v(t) = g*t
        Cell<Double> velocity = time.map(seconds -> 9.81 * seconds);
        //s(t) = 1/2 * g * t^2
        Cell<Double> height = time.map(seconds -> startHeightMeter - 1.5 * 9.81 * seconds * seconds);


        SLabel lblSeconds = new SLabel(time.map(value -> Double.toString(value) + " s"));
        SLabel lblSpeed = new SLabel(velocity.map(value -> Double.toString(value) + " m/s"));
        SLabel lblHeight = new SLabel(height.map(value -> Double.toString(value) + " m"));

        frame.add(lblSeconds);
        frame.add(lblSpeed);
        frame.add(lblHeight);
        frame.setSize(400, 160);
        frame.setVisible(true);
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
