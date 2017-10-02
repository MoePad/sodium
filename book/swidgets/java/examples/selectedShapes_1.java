import javax.swing.*;
import java.awt.FlowLayout;
import swidgets.*;
import nz.sodium.*;

public class selectedShapes_1 {
    public static void main(String[] args) {
        JFrame frame = new JFrame("selected shape");
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setLayout(new FlowLayout());

        SButton shape_1 = new SButton("Shape 1");
        SButton emptySpace = new SButton("Empty Space");

        Stream<String> shape1Selected = shape_1.sClicked.map(u -> "Shape 1 selected");
        Stream<String> shape1AndSpace = shape1Selected.merge(emptySpace.sClicked.map(u -> ""), (s1, s2) -> "never happens");

        SLabel lblSelection = new SLabel(shape1AndSpace.hold(""));

        frame.add(shape_1);
        frame.add(emptySpace);
        frame.add(lblSelection);
        frame.setSize(400, 160);
        frame.setVisible(true);
    }
    private static Integer parseInt(String t) {
        try {
            return Integer.parseInt(t);
        } catch (NumberFormatException e) {
            return 0;
        }
    }
}
