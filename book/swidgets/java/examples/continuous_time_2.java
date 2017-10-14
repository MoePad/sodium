import javax.swing.*;
import java.awt.FlowLayout;
import swidgets.*;
import nz.sodium.*;
import nz.sodium.time.*;
import java.util.*;

public class continuous_time_1 {
    public static void main(String[] args) throws InterruptedException {
        setUpLogic();
        loop();
    }

    private static void setUpLogic() {
        JFrame frame = new JFrame("Continuous Time 2");
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setLayout(new FlowLayout());

        TimerSystem timerSystem = new SecondsTimerSystem();
        Cell<Double> time = timerSystem.time;

        SLabel lblValue = new SLabel(time.map(value -> Double.toString(value)));

        frame.add(lblValue);
        frame.setSize(400, 160);
        frame.setVisible(true);
    }

    private static void loop() throws InterruptedException {
        long systemSampleRate = 1000L;
        while(true) {
            //jede Transaktion aktualisert TimerSystem.time
            Transaction.runVoid(() -> {});
            Thread.sleep(systemSampleRate);
        }
    }
}
