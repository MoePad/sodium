import javax.swing.*;
import java.awt.FlowLayout;
import swidgets.*;
import nz.sodium.*;
import nz.sodium.time.*;
import java.util.*;

public class ball_sandbox {
    public static void main(String[] args) throws InterruptedException {
        setUpLogic();
        loop();
    }

    private static void setUpLogic() {
        JFrame frame = new JFrame("Ball");
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setLayout(new BoxLayout(frame.getContentPane(), BoxLayout.Y_AXIS));

        frame.setSize(400, 160);
        frame.setVisible(true);
    }

    private static void loop() throws InterruptedException {
        long systemSampleRate = 1000L;
        StreamSink<Unit> sMain = new StreamSink<>();
        while(true) {
            //jede Transaktion aktualisiert TimerSystem.time (Sodium spezifisch) -> send löst Transaktion aus
            sMain.send(Unit.UNIT);
            Thread.sleep(systemSampleRate);
        }
    }

}
