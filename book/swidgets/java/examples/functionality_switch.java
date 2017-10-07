import javax.swing.*;
import java.awt.FlowLayout;
import swidgets.*;
import nz.sodium.*;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

public class functionality_switch {

    public static void main(String[] args) {
        JFrame frame = new JFrame("Dynamic List");
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setLayout(new FlowLayout());

        SButton func1Btn = new SButton("Func_1");
        SButton func2Btn = new SButton("Func_2");
        SButton fireBtn = new SButton("Fire");

        Stream<String> func1 = fireBtn.sClicked.map(u -> "Func 1");
        Stream<String> func2 = fireBtn.sClicked.map(u -> "Func 2");

        Cell<Stream<String>> functionality = func1Btn.sClicked.map(u -> func1)
                                                .orElse(func2Btn.sClicked.map(u -> func2))
                                                .hold(new Stream<String>());

        SLabel lblTest = new SLabel(Cell.switchS(functionality).hold("Start value."));

        frame.add(func1Btn);
        frame.add(func2Btn);
        frame.add(fireBtn);
        frame.add(lblTest);

        frame.setSize(400, 160);
        frame.setVisible(true);
    }
}
