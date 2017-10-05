import javax.swing.*;
import java.awt.FlowLayout;
import swidgets.*;
import nz.sodium.*;

public class avreceiver {
    public static void main(String[] args) {
        JFrame frame = new JFrame("AV Receiver");
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setLayout(new FlowLayout());

        Cell<String> hdmiCell = new Cell<>("HDMI stream");
        Cell<String> avCell = new Cell<>("AV stream");
        Cell<String> auxCell = new Cell<>("AUX stream");

        SButton hdmi = new SButton("HDMI");
        SButton av = new SButton("AV");
        SButton aux = new SButton("AUX");

        Stream<Cell<String>> hdmiStream = hdmi.sClicked.map(u -> hdmiCell);
        Stream<Cell<String>> avStream = av.sClicked.map(u -> avCell);
        Stream<Cell<String>> auxStream = aux.sClicked.map(u -> auxCell);

        Cell<Cell<String>> currentSelection = hdmiStream.merge(avStream, (c1, c2) -> c1)
                                     .merge(auxStream, (c1, c2) -> c1)
                                     .hold(hdmiCell);

        SLabel label = new SLabel(Cell.switchC(currentSelection));

        frame.add(hdmi);
        frame.add(av);
        frame.add(aux);
        frame.add(label);
        frame.setSize(400, 160);
        frame.setVisible(true);
    }
}
